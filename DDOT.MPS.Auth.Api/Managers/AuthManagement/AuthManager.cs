using AutoMapper;
using Core.CommonDtos;
using Core.CoreSettings;
using Core.Enums;
using Core.Utilities;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Model.Dtos;
using Model.Response;
using Model.Services.UserManagement;
using static Core.Exceptions.UserDefinedException;

namespace DDOT.MPS.Auth.Api.Managers

{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly IMpsUserRepository _userRepository;
        private readonly IExternalUserService _externalUserService;
        private readonly GlobalAppSettings _globalAppSettings;
        private readonly IAppUtils _appUtils;
        private readonly IJwtUtils _jwtUtils;
        private readonly IMpsAuthManagementRepository _authManagementRepository;

        public AuthManager(IMpsAuthManagementRepository authManagementRepository, IMpsUserRepository userRepository, IExternalUserService externalUserService, IMapper mapper, IOptions<GlobalAppSettings> globalAppSettings, IAppUtils appUtils, IJwtUtils jwtUtils)
        {
            _authManagementRepository = authManagementRepository;
            _userRepository = userRepository;
            _externalUserService = externalUserService;
            _mapper = mapper;
            _globalAppSettings = globalAppSettings.Value;
            _appUtils = appUtils;
            _jwtUtils = jwtUtils;
        }

        public async Task<BaseResponse<B2CTokenResponse>> Login(UserLoginDto userLoginDto)
        {
            MpsUser? dbUser = await _userRepository.GetUserByEmail(userLoginDto.Email);

            if (dbUser == null)
            {
                throw new UDValiationException("USER_NOT_FOUND");
            }

            if (!dbUser.IsActive)
            {
                throw new UDValiationException("USER_NOT_ACTIVE");
            }

            B2CTokenResponse response;
            if (dbUser.IsAdmin)
            {

                response = await _externalUserService.InitiateLogin(userLoginDto, Core.Enums.UserType.Admin);
            }
            else
            {
                response = await _externalUserService.InitiateLogin(userLoginDto, Core.Enums.UserType.Client);
            }

            return new BaseResponse<B2CTokenResponse> { Success = true, Data = response, Message = "USER_LOGIN_SUCCESSFULLY" };
        }

        // More optimized way to login. 
        // In this method handling both login validation and OTP generation in the same function
        public async Task<BaseResponse<OtpGenerateResponseDto>> LoginV2(UserLoginDto userLoginDto)
        {
            MpsUser? dbUser = await _userRepository.GetUserByEmail(userLoginDto.Email);
            User b2cUser;

            if (dbUser == null)
            {

               throw new UDValiationException("USER_NOT_FOUND");
            }

            if (!dbUser.IsActive)
            {
                throw new UDValiationException("USER_NOT_ACTIVE");
            }

            // Check if account is locked and within 2-hour lock period
            if (dbUser.IsAccountLocked)
            {
                throw new UDValiationException("ACCOUNT_LOCKED_DEFAULT");
            }

            Core.Enums.UserType userType = dbUser.IsAdmin ? Core.Enums.UserType.Admin : Core.Enums.UserType.Client;
            b2cUser = await _externalUserService.GetUserBySignInName(userLoginDto.Email, userType);

            if (b2cUser == null)
            {
                throw new UDValiationException("USER_NOT_FOUND");
            }

            // Reset failed login attempts if more than 2 hours have passed since the last failed attempt
            if (dbUser.LoginFailAttempts.HasValue && dbUser.LoginFailAttempts.Value < 5 &&
                dbUser.LastAccountLockTime.HasValue && dbUser.LastAccountLockTime.Value.AddHours(2) < DateTime.Now)
            {
                dbUser.LoginFailAttempts = 0;
            }

            B2CTokenResponse b2cResponse = null;

            try
            {
                if (dbUser.IsAdmin)
                {
                    b2cResponse = await _externalUserService.InitiateLogin(userLoginDto, Core.Enums.UserType.Admin);
                }
                else
                {
                    b2cResponse = await _externalUserService.InitiateLogin(userLoginDto, Core.Enums.UserType.Client);
                }
            }
            catch (UDValiationException ex)
            {
                if (ex.Message == "EMAIL_PASSWORD_INCORRECT")
                {
                    dbUser.LoginFailAttempts = (dbUser.LoginFailAttempts ?? 0) + 1;
                    dbUser.ModifiedDate = DateTime.UtcNow;

                    if (dbUser.LoginFailAttempts >= 5)
                    {
                        dbUser.IsAccountLocked = true;
                        dbUser.LastAccountLockTime = DateTime.UtcNow;
                        await _userRepository.UpdateUser(dbUser);

                        return new BaseResponse<OtpGenerateResponseDto> { 
                            Success = false, 
                            Data = new OtpGenerateResponseDto()
                            {
                                Email = dbUser.EmailAddress,
                                Firstname = dbUser.FirstName,
                                Lastname = dbUser.LastName,
                                Languagecode = dbUser.LanguageCode,
                            }, 
                            Message = "ACCOUNT_LOCKED" };
                    }

                    await _userRepository.UpdateUser(dbUser);

                    if (dbUser.LoginFailAttempts == 3)
                    {
                        throw new UDValiationException("FAILED_3_ATTEMPTS");
                    }

                    if (dbUser.LoginFailAttempts == 4)
                    {
                        throw new UDValiationException("FAILED_4_ATTEMPTS");
                    }

                    throw new UDValiationException("EMAIL_PASSWORD_INCORRECT");
                }
                throw;
            }

            // Reset login attempts on successful login
            dbUser.LoginFailAttempts = 0;

            dbUser.Otp = AuthUtils.Generate6DigitOTP();
            dbUser.OtpGeneratedOn = DateTime.Now;
            dbUser.OtpIncorrectCount = 0; // Set OTP incorrect count to 0 when request new OTP
            UserDto updatedUser = await _userRepository.UpdateUser(dbUser);

            OtpGenerateResponseDto responseDto = new OtpGenerateResponseDto()
            {
                Email = updatedUser.Emailaddress,
                Otp = updatedUser.Otp.Value,
                Firstname = updatedUser.Firstname,
                Lastname = updatedUser.Lastname,
                Languagecode = updatedUser.Languagecode,
                AccessToken = b2cResponse.AccessToken
            };

            return new BaseResponse<OtpGenerateResponseDto> { Success = true, Data = responseDto, Message = "OTP_GENERATED" };
        }

        public async Task<BaseResponse<UserResponseDto>> UserCheck(string email)
        {
            MpsUser? dbUser = await _userRepository.GetUserByEmail(email);
            User user;

            if (dbUser == null)
            {
                throw new UDValiationException("NO_ACCOUNT_FOUND");
            }

            if (dbUser.IsAdmin)
            {
                user = await _externalUserService.GetUserBySignInName(email, Core.Enums.UserType.Admin);
            }
            else
            {
                user = await _externalUserService.GetUserBySignInName(email, Core.Enums.UserType.Client);
            }

            if (user == null)
            {
                throw new UDValiationException("NO_ACCOUNT_FOUND");
            }

            bool isActive = await _userRepository.IsActiveUser(email);
            if(!isActive)
            {
                throw new UDValiationException("USER_NOT_ACTIVE");
            }

            UserResponseDto userDto = _mapper.Map<UserResponseDto>(dbUser);

            return new BaseResponse<UserResponseDto> { Success = true, Data = userDto, Message = "USER_EXIST" };
        }
        
        public async Task<BaseResponse<ResetPasswordResponseDto>> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            MpsUser user = await _userRepository.GetUserByEmail(resetPasswordDto.Emailaddress);

            if (user == null)
            {
                throw new UDValiationException("USER_NOT_REGISTERED");
            }

            if (resetPasswordDto.Mobilenumber != null)
            {
                if (!_appUtils.IsValidPhoneNumber(resetPasswordDto.Mobilenumber))
                {
                    throw new UDValiationException("MOBILE_NO_INVALID");
                }
                user.MobileNumber = resetPasswordDto.Mobilenumber;
            }
            user.IsInitialPasswordReset = true;
            user.IsResetPasswordLinkUsed = true;
            UserDto updatedUser = await _userRepository.UpdateUser(user);
            UserResponseDto userDto = _mapper.Map<UserResponseDto>(updatedUser);
            #region // Reset password in B2C Tenent
            // Need to be finalized once the B2C configuration is completed
            #endregion

            // Login the user
            UserLoginDto userLoginDto = new UserLoginDto
            {
                Email = resetPasswordDto.Emailaddress,
                Password = resetPasswordDto.Password
            };

            B2CTokenResponse b2CTokenResponse;

            if (user.IsAdmin)
            {
                await _externalUserService.ResetUserPassword(resetPasswordDto.Emailaddress, resetPasswordDto.Password, Core.Enums.UserType.Admin);
                b2CTokenResponse = await _externalUserService.InitiateLogin(userLoginDto, Core.Enums.UserType.Admin);
            }
            else
            {
                await _externalUserService.ResetUserPassword(resetPasswordDto.Emailaddress, resetPasswordDto.Password, Core.Enums.UserType.Client);
                b2CTokenResponse = await _externalUserService.InitiateLogin(userLoginDto, Core.Enums.UserType.Client);
            }

            //B2CTokenResponse 
            b2CTokenResponse.LoginToken = _jwtUtils.GenerateToken(userDto.Emailaddress, _globalAppSettings.LoginTokenExpiryHours);

            ResetPasswordResponseDto combinedResponse = new ResetPasswordResponseDto
            {
                User = userDto,
                LoginResponse = b2CTokenResponse
            };

            return new BaseResponse<ResetPasswordResponseDto> { Success = true, Data = combinedResponse, Message = "PASSWORD_CHANGED_SUCCESSFULLY" };
        }

        public async Task<BaseResponse<string>> UserIsInitialPasswordResetCheck(string emailToken)
        {
            bool isEmailTokenValid = _jwtUtils.IsTokenValid(emailToken);
            if (!isEmailTokenValid)
            {
                throw new UDValiationException("RESET_PASSWORD_LINK_INVALID");
            }

            //decode emailToken
            TokenResponse decodedToken = _jwtUtils.DecodeToken(emailToken);
            string email = decodedToken.Email;

            bool user = await _userRepository.IsExistingUserByEmail(email);
            if (!user)
            {
                throw new UDValiationException("USER_NOT_REGISTERED");
            }

            bool isResetPasswordLinkUsed = await _userRepository.IsResetPasswordLinkUsed(email);
            if (isResetPasswordLinkUsed)
            {
                throw new UDValiationException("RESET_PASSWORD_LINK_USED");
            }

            bool isInitialPasswordReset = await _userRepository.IsInitialPasswordReset(email);
            bool isMigratedAccount = await _userRepository.IsMigratedAccount(email);
            bool isMobileNoCapturing = isInitialPasswordReset && isMigratedAccount;

            return new BaseResponse<string> { Success = true, Data = isMobileNoCapturing.ToString() , Message = "INITIAL_PASSWORD_RESET_CHECK_SUCCESSFUL" };
        }

        public async Task<BaseResponse<OtpGenerateResponseDto>> GenerateOTP(string email)
        {
            MpsUser? user = await _userRepository.GetUserByEmail(email);

            if (user == null)
            {
                throw new UDValiationException("USER_NOT_FOUND");
            }

            User b2cUser;
            if (user.IsAdmin)
            {
                b2cUser = await _externalUserService.GetUserBySignInName(email, Core.Enums.UserType.Admin);
            }
            else
            {
                b2cUser = await _externalUserService.GetUserBySignInName(email, Core.Enums.UserType.Client);
            }

            if (b2cUser == null)
            {
                throw new UDValiationException("USER_NOT_FOUND");
            }

            int otp = AuthUtils.Generate6DigitOTP();
            user.Otp = otp;
            user.OtpGeneratedOn = DateTime.Now;
            user.OtpIncorrectCount = 0; // Set OTP incorrect count to 0 when request new OTP
            await _userRepository.UpdateUser(user);

            return new BaseResponse<OtpGenerateResponseDto> { Success = true, Data = new OtpGenerateResponseDto { Email = email, Otp = otp, Firstname = user.FirstName, Lastname = user.LastName, Languagecode = user.LanguageCode }, Message = "OTP_GENERATED" };
        }

        public async Task<BaseResponse<OtpVerifyResponseDto>> VerifyOtp(string email, int otp)
        {
            MpsUser? user = await _userRepository.GetUserByEmail(email);

            if (user == null)
            {
                throw new UDValiationException("USER_NOT_FOUND");
            }

            User b2cUser;
            if (user.IsAdmin)
            {
                b2cUser = await _externalUserService.GetUserBySignInName(email, Core.Enums.UserType.Admin);
            }
            else
            {
                b2cUser = await _externalUserService.GetUserBySignInName(email, Core.Enums.UserType.Client);
            }

            if (b2cUser == null)
            {
                throw new UDValiationException("USER_NOT_FOUND");
            }

            if (user.OtpIncorrectCount.HasValue && user.OtpIncorrectCount.Value >= _globalAppSettings.MaxOtpIncorrectCount)
            {
                throw new UDValiationException("INCORRECT_OTP_MANYTIMES");
            }

            if (user.Otp != otp)
            {
                user.OtpIncorrectCount = (int?)(user.OtpIncorrectCount + 1); // increment OTP incorrect count by 1
                await _userRepository.UpdateUser(user);
                throw new UDValiationException("CODE_NOT_WORK");
            }

            if ((DateTime.Now - user.OtpGeneratedOn.Value).TotalMinutes > _globalAppSettings.OtpExpirationTime)
            {
                throw new UDValiationException("CODE_EXPIRED");
            }

            string token = _jwtUtils.GenerateToken(email, _globalAppSettings.LoginTokenExpiryHours);

            return new BaseResponse<OtpVerifyResponseDto> { Success = true, Data = new OtpVerifyResponseDto { Email = email, Token = token, Verified = true }, Message = "OTP_VERIFIED" };
        }

        public async Task<BaseResponse<UserTokenResponse>> GenerateResetPasswordToken(string email)
        {
            MpsUser? dbUser = await _userRepository.GetUserByEmail(email);

            if (dbUser == null) 
            {
                throw new UDValiationException("NO_ACCOUNT_WITH_EMAIL");
            }

            User? user;
            if (dbUser.IsAdmin)
            {
                user = await _externalUserService.GetUserBySignInName(email, Core.Enums.UserType.Admin);
            }
            else
            {
                user = await _externalUserService.GetUserBySignInName(email, Core.Enums.UserType.Client);
            }

            if (user == null)
            {
                throw new UDValiationException("NO_ACCOUNT_WITH_EMAIL");
            }

            if (!dbUser.IsActive)
            {
                throw new UDValiationException("USER_NOT_ACTIVE");
            }

            if (dbUser.IsAccountLocked)
            {
                throw new UDValiationException("ACCOUNT_LOCKED_DEFAULT");
            }

            string token = _jwtUtils.GenerateToken(email,_globalAppSettings.ResetPasswordTokenExpiryHours);

            dbUser.IsResetPasswordLinkUsed = false;
            UserDto updatedUser = await _userRepository.UpdateUser(dbUser);
            UserResponseDto userDto = _mapper.Map<UserResponseDto>(updatedUser);

            UserTokenResponse userTokenResponse = new UserTokenResponse
            {
                Token = token,
                User = userDto
            };

            return new BaseResponse<UserTokenResponse> { Success = true, Data = userTokenResponse, Message = "TOKEN_GENERATED" };
        }

        public async Task<BaseResponse<UserRolesWithPermissionsAccessToken>> GenerateAccessToken(string email)
        {
            MpsUser? dbUser = await _userRepository.GetUserByEmail(email);

            if (dbUser == null)
            {
                throw new UDValiationException("NO_ACCOUNT_WITH_EMAIL");
            }

            User? user;
            if (dbUser.IsAdmin)
            {
                user = await _externalUserService.GetUserBySignInName(email, Core.Enums.UserType.Admin);
            }
            else
            {
                user = await _externalUserService.GetUserBySignInName(email, Core.Enums.UserType.Client);
            }

            if (user == null)
            {
                throw new UDValiationException("NO_ACCOUNT_WITH_EMAIL");
            }

            if (!dbUser.IsActive)
            {
                throw new UDValiationException("USER_NOT_ACTIVE");
            }

            if (dbUser.IsAccountLocked)
            {
                throw new UDValiationException("ACCOUNT_LOCKED_DEFAULT");
            }

            int userId = dbUser.UserId;

            List<UserRole> userRoles = await _authManagementRepository.GetUserRolesByUserId(userId);
            AuthUserDto authUserDto;
            if (userRoles == null || !userRoles.Any())
            {
                authUserDto = new AuthUserDto
                {
                    Userid = dbUser.UserId,
                    Firstname = dbUser.FirstName,
                    Lastname = dbUser.LastName,
                    Emailaddress = dbUser.EmailAddress,
                    RoleId = null,
                    Mobilenumber = dbUser.MobileNumber
                };
            }
            else
            {
                authUserDto = new AuthUserDto
                {
                    Userid = dbUser.UserId,
                    Firstname = dbUser.FirstName,
                    Lastname = dbUser.LastName,
                    Emailaddress = dbUser.EmailAddress,
                    RoleId = userRoles.First().Role.RoleId,
                    Mobilenumber = dbUser.MobileNumber
                };
            }

            
            string token = _jwtUtils.GenerateToken(authUserDto, _globalAppSettings.LoginTokenExpiryHours);

            UserRolesWithPermissionsAccessToken userTokenResponse = new UserRolesWithPermissionsAccessToken
            {
                AccessToken = token
            };

            return new BaseResponse<UserRolesWithPermissionsAccessToken> { Success = true, Data = userTokenResponse, Message = "ACCESS_TOKEN_GENERATED_SUCCESSFULLY" };
        }
        public async Task<BaseResponse<List<UserRoleWithPermissionsDto>>> GetRolesAndPermissionsByUserId(int userId)
        {
            var roles = await _authManagementRepository.GetUserRolesAndPermissionsByUserId(userId);
            var rolesDto = _mapper.Map<List<UserRoleWithPermissionsDto>>(roles) ;
            return new BaseResponse<List<UserRoleWithPermissionsDto>> { Success = true,Data = rolesDto };
        }
    }
}

using AutoMapper;
using Core.CoreSettings;
using Core.Enums;
using Core.Utilities;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Model.Dtos;
using Model.Request;
using Model.Response;
using Model.Services.UserManagement;
using System.Text.Json;
using static Core.Exceptions.UserDefinedException;

namespace DDOT.MPS.Auth.Api.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IMapper _mapper;
        private readonly IMpsUserRepository _userRepository;
        private readonly IExternalUserService _externalUserService;
        private readonly GlobalAppSettings _globalAppSettings;
        private readonly IAppUtils _appUtils;
        private readonly IJwtUtils _jwtUtils;
        private readonly ILogger<UserManager> _logger;
        private readonly IMpsAgencyRepository _mpsAgencyRepository;

        public UserManager(IMpsUserRepository userRepository, IExternalUserService externalUserService, IMapper mapper, IOptions<GlobalAppSettings> globalAppSettings, IAppUtils appUtils, IJwtUtils jwtUtils, ILogger<UserManager> logger, IMpsAgencyRepository mpsAgencyRepository)
        {
            _userRepository = userRepository;
            _externalUserService = externalUserService;
            _mapper = mapper;
            _globalAppSettings = globalAppSettings.Value;
            _appUtils = appUtils;
            _jwtUtils = jwtUtils;
            _logger = logger;
            _mpsAgencyRepository = mpsAgencyRepository;
        }

        public async Task<BaseResponse<UserResponseDto>> CreateUser(UserDto user)
        {
            _logger.LogInformation("DDOT.MPS.Auth.Api.Managers.UserManagement.UserManager.CreateUser | Request in progress.");

            User? b2cUser = await _externalUserService.GetUserBySignInName(user.Emailaddress, user.UserType ?? Core.Enums.UserType.Client);
            _logger.LogInformation("DDOT.MPS.Auth.Api.Managers.UserManagement.UserManager.CreateUser | B2C user retrieved. User Data: {b2cUser}", JsonSerializer.Serialize(b2cUser));

            MpsUser? userData = await _userRepository.GetUserByEmail(user.Emailaddress);
            _logger.LogInformation("DDOT.MPS.Auth.Api.Managers.UserManagement.UserManager.CreateUser | DB user retrieved. User Data: {userData}", JsonSerializer.Serialize(userData));

            if (userData != null)
            {
                // User should be able to continue the registration flow if that user has not verified their email address.
                if (!userData.IsEmailVerified)
                {
                    userData.FirstName = user.Firstname;
                    userData.LastName = user.Lastname;
                    userData.LanguageCode = user.Languagecode;
                    userData.MobileNumber = user.Mobilenumber;
                    userData.Otp = AuthUtils.Generate6DigitOTP();
                    userData.OtpGeneratedOn = DateTime.Now;
                    await _userRepository.UpdateUser(userData);
                    UserResponseDto userResponse = _mapper.Map<UserResponseDto>(userData);
                    return new BaseResponse<UserResponseDto> { Success = true, Data = userResponse, Message = "OTP_GENERATED" };
                }
                throw new UDValiationException("EMAIL_ALREADY_USED");
            }

            if (b2cUser != null)
            {
                throw new UDValiationException("EMAIL_ALREADY_USED");
            }

            if (!_appUtils.IsValidPassword(user.Password))
            {
                throw new UDValiationException("WEAK_PASSWORD");
            }

            if (!_appUtils.IsValidPhoneNumber(user.Mobilenumber))
            {
                throw new UDValiationException("MOBILE_INVALID");
            }

            if (user.UserTypeId == null)
            {
                throw new UDValiationException("USER_TYPE_REQUIRED");
            }

            string? userType = await _userRepository.GetUserType(user.UserTypeId);

            if (userType != null && userType.ToLower().Equals("company") && user.AgencyId == null)
            {
                throw new UDValiationException("AGENCY_REQUIRED");
            }

            #region // Create user in B2C Tenant
            
            UserRegistrationB2cDto userRegistrationB2CDto = new UserRegistrationB2cDto()
            {
                FirstName = user.Firstname,
                LastName = user.Lastname,
                Email = user.Emailaddress,
                Password = user.Password,
                ContactNumber = user.Mobilenumber,
            };

            User createdUser = await _externalUserService.CreateNewUser(userRegistrationB2CDto, user.UserType ?? Core.Enums.UserType.Client);

            if (createdUser == null)
            {
                _logger.LogError("DDOT.MPS.Auth.Api.Managers.UserManagement.UserManager.CreateUser | Error in B2C user creation.");
                throw new UDValiationException("Error occurred while creating user in B2C Tenant.");
            }

            _logger.LogInformation("DDOT.MPS.Auth.Api.Managers.UserManagement.UserManager.CreateUser | User successfully created in B2C.");

            #endregion

            // generate OTP
            user.Otp = AuthUtils.Generate6DigitOTP();
            user.Otpgeneratedon = DateTime.Now;

            user.UserType = null;

            MpsUser mpsUser = _mapper.Map<MpsUser>(user);
            await _userRepository.CreateUser(mpsUser);            

            UserResponseDto userResponseDto = _mapper.Map<UserResponseDto>(mpsUser);

            _logger.LogInformation("DDOT.MPS.Auth.Api.Managers.UserManagement.UserManager.CreateUser | User successfully created in database.");
            return new BaseResponse<UserResponseDto> { Success = true, Data = userResponseDto, Message = "USER_REGISTER_SUCCESSFUL" };
        }

        public async Task<BaseResponse<UserResponseDto>> CreateAdmin(UserDto user)
        {
            User? b2cUser = await _externalUserService.GetUserBySignInName(user.Emailaddress, Core.Enums.UserType.Admin);
            MpsUser? userData = await _userRepository.GetUserByEmail(user.Emailaddress);

            if (userData != null)
            {
                // User should be able to continue the registration flow if that user has not verified their email address.
                if (!userData.IsEmailVerified)
                {
                    userData.FirstName = user.Firstname;
                    userData.LastName = user.Lastname;
                    userData.LanguageCode = user.Languagecode;
                    userData.MobileNumber = user.Mobilenumber;
                    userData.Otp = AuthUtils.Generate6DigitOTP();
                    userData.OtpGeneratedOn = DateTime.Now;
                    await _userRepository.UpdateUser(userData);
                    UserResponseDto userResponse = _mapper.Map<UserResponseDto>(userData);
                    return new BaseResponse<UserResponseDto> { Success = true, Data = userResponse, Message = "OTP_GENERATED" };
                }
                throw new UDValiationException("EMAIL_ALREADY_USED");
            }

            if (b2cUser != null)
            {
                throw new UDValiationException("EMAIL_ALREADY_USED");
            }

            if (!_appUtils.IsValidPassword(user.Password))
            {
                throw new UDValiationException("WEAK_PASSWORD");
            }

            if (!_appUtils.IsValidPhoneNumber(user.Mobilenumber))
            {
                throw new UDValiationException("MOBILE_INVALID");
            }

            #region // Create user in B2C Tenant
            UserRegistrationB2cDto userRegistrationB2CDto = new UserRegistrationB2cDto()
            {
                FirstName = user.Firstname,
                LastName = user.Lastname,
                Email = user.Emailaddress,
                Password = user.Password,
                ContactNumber = user.Mobilenumber,
            };

            User createdUser = await _externalUserService.CreateNewUser(userRegistrationB2CDto, Core.Enums.UserType.Admin);

            if (createdUser == null)
            {
                throw new UDValiationException("Error occurred while creating user in B2C Tenant.");
            }
            #endregion

            // generate OTP
            user.Otp = AuthUtils.Generate6DigitOTP();
            user.Otpgeneratedon = DateTime.Now;

            MpsUser mpsUser = _mapper.Map<MpsUser>(user);
            await _userRepository.CreateUser(mpsUser);
            UserResponseDto userResponseDto = _mapper.Map<UserResponseDto>(mpsUser);
            return new BaseResponse<UserResponseDto> { Success = true, Data = userResponseDto, Message = "USER_REGISTER_SUCCESSFUL" };
        }

            public async Task<BaseResponse<UserResponseDto>> ValidateOTP(int userId, int otp)
        {
            MpsUser? user = await _userRepository.GetUserById(userId);

            if(user == null)
            {
                throw new UDValiationException("USER_NOT_FOUND");
            }

            User b2cUser;
            if (user.IsAdmin)
            {
                b2cUser = await _externalUserService.GetUserBySignInName(user.EmailAddress, Core.Enums.UserType.Admin);
            }
            else
            {
                b2cUser = await _externalUserService.GetUserBySignInName(user.EmailAddress, Core.Enums.UserType.Client);
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
                user.OtpIncorrectCount = (int?)(user.OtpIncorrectCount + 1); // Increment OTP incorrect count by 1
                await _userRepository.UpdateUser(user);
                throw new UDValiationException("CODE_NOT_WORK");
            }

            if ((DateTime.Now - user.OtpGeneratedOn.Value).TotalMinutes > _globalAppSettings.OtpExpirationTime)
            {
                throw new UDValiationException("CODE_EXPIRED");
            }

            user.IsEmailVerified = true;
            user.IsActive = true;
            await _userRepository.UpdateUser(user);

            string token = _jwtUtils.GenerateToken(user.EmailAddress, _globalAppSettings.LoginTokenExpiryHours);

            UserResponseDto userResponseDto = _mapper.Map<UserResponseDto>(user);
            userResponseDto.Token = token;
            return new BaseResponse<UserResponseDto> { Success = true, Data = userResponseDto, Message = "OTP_VERIFIED" };
        }

        public async Task<BaseResponse<Result<UserDetailsDto>>> GetPaginatedList(UserPaginatedRequest request)
        {
            IQueryable<MpsUser> users = _userRepository.GetAll(request);
            List<UserDetailsDto> userResponseDtos = await users.Select(x => _mapper.Map<UserDetailsDto>(x)).ToListAsync();

            BaseResponse<Result<UserDetailsDto>> response = new BaseResponse<Result<UserDetailsDto>>
            {
                Success = true,
                Data = new Result<UserDetailsDto>
                {
                    Entities = userResponseDtos.ToArray(),
                    Pagination = new Pagination()
                    {
                        Length = _userRepository.GetRowCount(request),
                        PageSize = request.PagingAndSortingInfo.Paging.PageSize
                    }
                }
            };

            return response;
        }

        public async Task<BaseResponse<UserTypesAndAgenciesDto>> GetUserTypesAndAgencies()
        {
            UserTypesAndAgenciesDto userTypesAndAgenciesDto = _mpsAgencyRepository.GetUserTypesAndAgencies();
            return new BaseResponse<UserTypesAndAgenciesDto> { Message = "USER_TYPES_AND_AGENCIES_RETRIEVED_SUCCESSFULLY", Success = true, Data = userTypesAndAgenciesDto };
        }
    }
}

using AutoMapper;
using Core.CoreSettings;
using Core.Utilities;
using DataAccess.Entities;
using DataAccess.Repositories;
using DDOT.MPS.Auth.Api.Managers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Model.Dtos;
using Model.Response;
using Model.Services.UserManagement;
using Moq;
using static Core.Exceptions.UserDefinedException;

namespace Test.Managers
{

    [TestFixture]
    public class AuthManagerTest
    {
        private IAuthManager _authManager;
        private Mock<IMapper> _mapper;
        private Mock<IMpsUserRepository> _userRepository;
        private Mock<IExternalUserService> _externalUserService;
        private IOptions<GlobalAppSettings> _globalAppSettings;
        private Mock<IAppUtils> _appUtils;
        private Mock<IJwtUtils> _jwtUtils;
        private Mock<IMpsAuthManagementRepository> _authManagementRepository;
        private AuthUtils _authUtils;

        [SetUp]
        public void SetUp()
        {
            _mapper = new Mock<IMapper>();
            _userRepository = new Mock<IMpsUserRepository>();
            _externalUserService = new Mock<IExternalUserService>();
            _appUtils = new Mock<IAppUtils>();
            _jwtUtils = new Mock<IJwtUtils>();
            _authManagementRepository = new Mock<IMpsAuthManagementRepository>();
            _globalAppSettings = Options.Create(new GlobalAppSettings { OtpExpirationTime = 5, MaxOtpIncorrectCount = 3 });
            _authManager = new AuthManager(_authManagementRepository.Object, _userRepository.Object, _externalUserService.Object, _mapper.Object, _globalAppSettings, _appUtils.Object, _jwtUtils.Object);
        }

        [Test]
        public async Task Login_UserNotActive()
        {
            UserLoginDto userLoginDto = new UserLoginDto()
            {
                Email = "test@gmail.com",
                Password = "Test@123"
            };

            MpsUser mpsUser = new MpsUser()
            {
                FirstName = "Test",
                IsActive = false,
            };

            _userRepository.Setup(x => x.GetUserByEmail(userLoginDto.Email)).ReturnsAsync(mpsUser);

            var ex = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.Login(userLoginDto));
            Assert.That(ex.Message, Is.EqualTo("USER_NOT_ACTIVE"));
        }

        [Test]
        public async Task Login_Success()
        {
            UserLoginDto userLoginDto = new UserLoginDto()
            {
                Email = "test@gmail.com",
                Password = "Test@123"
            };

            MpsUser mpsUser = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
            };

            _userRepository.Setup(x => x.GetUserByEmail(userLoginDto.Email)).ReturnsAsync(mpsUser);
            _externalUserService.Setup(x => x.InitiateLogin(It.IsAny<UserLoginDto>(), Core.Enums.UserType.Client)).ReturnsAsync(new B2CTokenResponse());

            var result = await _authManager.Login(userLoginDto);

            Assert.IsTrue(result.Success);
            Assert.NotNull(result.Data);
            Assert.AreEqual("USER_LOGIN_SUCCESSFULLY", result.Message);
        }

        [Test]
        public async Task LoginV2_UserNotFound_Inactive_Locked()
        {
            UserLoginDto userLoginDto = new UserLoginDto()
            {
                Email = "test@gmail.com",
                Password = "Test@123"
            };

            MpsUser dbUserActive = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = true,
            };

            MpsUser dbUserInactive = new MpsUser()
            {
                FirstName = "Test",
                IsActive = false,
            };

            User b2cUser = new User
            {
                DisplayName = "Lakshan"
            };

            //only dbUser is null
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((MpsUser)null);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync(b2cUser);

            var ex1 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.LoginV2(userLoginDto));
            Assert.That(ex1.Message, Is.EqualTo("USER_NOT_FOUND"));

            //only b2cUser is null
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUserActive);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync((User)null);

            var ex2 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.LoginV2(userLoginDto));
            Assert.That(ex2.Message, Is.EqualTo("ACCOUNT_LOCKED_DEFAULT"));

            //Both users are null
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((MpsUser)null);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync((User)null);

            var ex3 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.LoginV2(userLoginDto));
            Assert.That(ex3.Message, Is.EqualTo("USER_NOT_FOUND"));

            //User is inactive
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUserInactive);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync(b2cUser);

            var ex4 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.LoginV2(userLoginDto));
            Assert.That(ex4.Message, Is.EqualTo("USER_NOT_ACTIVE"));

            //User account locked
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUserActive);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync(b2cUser);

            var ex5 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.LoginV2(userLoginDto));
            Assert.That(ex5.Message, Is.EqualTo("ACCOUNT_LOCKED_DEFAULT"));
        }

        [Test]
        public async Task LoginV2_Failed()
        {
            UserLoginDto userLoginDto = new UserLoginDto()
            {
                Email = "test@gmail.com",
                Password = "Test@123"
            };

            MpsUser dbUserAttempt12 = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = false,
                LoginFailAttempts = 1,
                LastAccountLockTime = null,
            };

            MpsUser dbUserAttemptsexceeded = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = false,
                LoginFailAttempts = 6,
                LastAccountLockTime = null,
            };

            MpsUser dbUserAttempt3 = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = false,
                LoginFailAttempts = 2,
                LastAccountLockTime = null,
            };

            MpsUser dbUserAttempt4 = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = false,
                LoginFailAttempts = 3,
                LastAccountLockTime = null,
            };

            User b2cUser = new User
            {
                DisplayName = "Lakshan"
            };

            //Login attempts exceeded
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUserAttemptsexceeded);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync(b2cUser);
            _externalUserService.Setup(x => x.InitiateLogin(It.IsAny<UserLoginDto>(), Core.Enums.UserType.Client)).ThrowsAsync(new UDValiationException("EMAIL_PASSWORD_INCORRECT"));
            _userRepository.Setup(x => x.UpdateUser(It.IsAny<MpsUser>()));

            var result = await _authManager.LoginV2(userLoginDto);
            
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.That(result.Message, Is.EqualTo("ACCOUNT_LOCKED"));

            //Login attempts = 3
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUserAttempt3);

            var ex = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.LoginV2(userLoginDto));
            Assert.That(ex.Message, Is.EqualTo("FAILED_3_ATTEMPTS"));

            //Login attempts = 4
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUserAttempt4);

            var ex2 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.LoginV2(userLoginDto));
            Assert.That(ex2.Message, Is.EqualTo("FAILED_4_ATTEMPTS"));

            //Login attempts = 1/2
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUserAttempt12);

            var ex3 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.LoginV2(userLoginDto));
            Assert.That(ex3.Message, Is.EqualTo("EMAIL_PASSWORD_INCORRECT"));

        }

        [Test]
        public async Task LoginV2_Success()
        {
            UserLoginDto userLoginDto = new UserLoginDto()
            {
                Email = "test@gmail.com",
                Password = "Test@123"
            };

            MpsUser dbUserActive = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = false,
                LoginFailAttempts = 0,
                LastAccountLockTime = null,
            };

            User b2cUser = new User
            {
                DisplayName = "Lakshan"
            };

            B2CTokenResponse b2CTokenResponse = new B2CTokenResponse()
            {
                AccessToken = "testToken",
                Email = "test@gmail.com",
                ExpiresIn = 3,
                IdTokenType = "test",
                LoginToken = "test",
                RefreshToken = "test",
                token_type = "test",
            };

            UserDto userDto = new UserDto()
            {
                Firstname = "Test",
                Emailaddress = "test@gmail.com",
                Otp = 123456,
                Lastname = "Test",
                Languagecode = "en",
            };

            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUserActive);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync(b2cUser);
            _externalUserService.Setup(x => x.InitiateLogin(It.IsAny<UserLoginDto>(), Core.Enums.UserType.Client)).ReturnsAsync(b2CTokenResponse);
            _userRepository.Setup(x => x.UpdateUser(It.IsAny<MpsUser>())).ReturnsAsync(userDto);
           

            var result = await _authManager.LoginV2(userLoginDto);
            
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.IsTrue(result.Success);
            Assert.That(result.Message, Is.EqualTo("OTP_GENERATED"));

            _userRepository.Verify(x => x.GetUserByEmail(It.Is<string>(email => email == userLoginDto.Email)), Times.Once);
            _externalUserService.Verify(x => x.GetUserBySignInName(It.Is<string>(email => email == userLoginDto.Email), Core.Enums.UserType.Client), Times.Once);
            _externalUserService.Verify(x => x.InitiateLogin(It.Is<UserLoginDto>(login => login.Email == userLoginDto.Email && login.Password == userLoginDto.Password), Core.Enums.UserType.Client), Times.Once);
            _userRepository.Verify(x => x.UpdateUser(It.IsAny<MpsUser>()), Times.Once);

        }

        [Test]
        public async Task UserCheck_NoUserFound()
        {
            string email = "test@gmail.com";

            MpsUser dbUser = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = false,
                LoginFailAttempts = 0,
                LastAccountLockTime = null,
            };

            User b2cUser = new User
            {
                DisplayName = "Lakshan"
            };

            //only dbUser is null
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((MpsUser)null);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync(b2cUser);

            var ex1 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.UserCheck(email));
            Assert.That(ex1.Message, Is.EqualTo("NO_ACCOUNT_FOUND"));

            //only b2cUser is null
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUser);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync((User)null);

            var ex2 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.UserCheck(email));
            Assert.That(ex2.Message, Is.EqualTo("NO_ACCOUNT_FOUND"));

            //Both users are null
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((MpsUser)null);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync((User)null);

            var ex3 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.UserCheck(email));
            Assert.That(ex3.Message, Is.EqualTo("NO_ACCOUNT_FOUND"));


        }

        [Test]
        public async Task UserCheck_UserNotActive()
        {
            string email = "test@gmail.com";

            MpsUser dbUser = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = false,
                LoginFailAttempts = 0,
                LastAccountLockTime = null,
            };

            User b2cUser = new User
            {
                DisplayName = "Lakshan"
            };

            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUser);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync(b2cUser);
            _userRepository.Setup(x => x.IsActiveUser(It.IsAny<string>())).ReturnsAsync(false);

            var ex = Assert.ThrowsAsync<UDValiationException>(async() => await _authManager.UserCheck(email));
            Assert.That(ex.Message, Is.EqualTo("USER_NOT_ACTIVE"));

        }

        [Test]
        public async Task UserCheck_Success()
        {
            string email = "test@gmail.com";

            MpsUser dbUser = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = false,
                LoginFailAttempts = 0,
                LastAccountLockTime = null,
            };

            User b2cUser = new User
            {
                DisplayName = "Lakshan"
            };

            UserResponseDto userResponseDto = new UserResponseDto()
            {
                Firstname = "Test",
                Emailaddress = "test@gmail.com"
            };

            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUser);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync(b2cUser);
            _userRepository.Setup(x => x.IsActiveUser(It.IsAny<string>())).ReturnsAsync(true);
            _mapper.Setup(x => x.Map<UserResponseDto>(dbUser)).Returns(userResponseDto);

            var result = await _authManager.UserCheck(email);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            Assert.That(result.Message, Is.EqualTo("USER_EXIST"));

        }

        [Test]
        public async Task ResetPassword_UserNotRegistered()
        {
            ResetPasswordDto resetPasswordDto = new ResetPasswordDto()
            {
                Emailaddress = "test@gmail.com",
                Mobilenumber = "+12562364521",
                Password = "Test@123"
            };

            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((MpsUser)null);

            var ex1 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.ResetPassword(resetPasswordDto));
            Assert.That(ex1.Message, Is.EqualTo("USER_NOT_REGISTERED"));

        }

        [Test]
        public async Task ResetPassword_InvalidPhoneNo()
        {
            ResetPasswordDto resetPasswordDto = new ResetPasswordDto()
            {
                Emailaddress = "test@gmail.com",
                Mobilenumber = "+162364521",
                Password = "Test@123"
            };

            MpsUser dbUser = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = false,
                LoginFailAttempts = 0,
                LastAccountLockTime = null,
                MobileNumber="+1254"
            };

            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUser);

            var ex1 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.ResetPassword(resetPasswordDto));
            Assert.That(ex1.Message, Is.EqualTo("MOBILE_NO_INVALID"));

        }

        [Test]
        public async Task ResetPassword_Success()
        {
            ResetPasswordDto resetPasswordDto = new ResetPasswordDto()
            {
                Emailaddress = "test@gmail.com",
                Mobilenumber = null,
                Password = "Test@123"
            };

            MpsUser dbUser = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = false,
                LoginFailAttempts = 0,
                LastAccountLockTime = null,
                MobileNumber = "2819374192"
            };

            UserDto userDto = new UserDto()
            {
                Firstname = "Test",
                Emailaddress = "test@gmail.com",
                Otp = 123456,
                Lastname = "Test",
                Languagecode = "en"    
            };

            UserResponseDto userResponseDto = new UserResponseDto()
            {
                Firstname = "Test",
                Emailaddress = "test@gmail.com"
            };

            B2CTokenResponse b2CTokenResponse = new B2CTokenResponse()
            {
                AccessToken = "testToken",
                Email = "test@gmail.com",
                ExpiresIn = 3,
                IdTokenType = "test",
                LoginToken = "test",
                RefreshToken = "test",
                token_type = "test",
            };

            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUser);
            _userRepository.Setup(x => x.UpdateUser(It.IsAny<MpsUser>())).ReturnsAsync(userDto);
            _mapper.Setup(x => x.Map<UserResponseDto>(It.IsAny<UserDto>())).Returns(userResponseDto);
            _externalUserService.Setup(x => x.ResetUserPassword(It.IsAny<string>(), It.IsAny<string>(), Core.Enums.UserType.Client));
            _externalUserService.Setup(x => x.InitiateLogin(It.IsAny<UserLoginDto>(), Core.Enums.UserType.Client)).ReturnsAsync(b2CTokenResponse);
            _jwtUtils.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<int>())).Returns("mock_login_token");
            
            var result = await _authManager.ResetPassword(resetPasswordDto);

            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.IsTrue(result.Success);
            Assert.That(result.Message, Is.EqualTo("PASSWORD_CHANGED_SUCCESSFULLY"));
        }

        [Test]
        public async Task UserIsInitialPasswordResetCheck_Failed()
        {
            string emailToken = "emailToken";
            TokenResponse tokenResponse = new TokenResponse()
            {
                Email = emailToken,
                Exp = 2,
                Iat = 3,
                Nbf = 4,
            };

            //Invalid token
            _jwtUtils.Setup(x => x.IsTokenValid(It. IsAny<string>())).Returns(false);

            var ex1 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.UserIsInitialPasswordResetCheck(emailToken));
            Assert.That(ex1.Message, Is.EqualTo("RESET_PASSWORD_LINK_INVALID"));

            //Non registered user
            _jwtUtils.Setup(x => x.IsTokenValid(It.IsAny<string>())).Returns(true);
            _jwtUtils.Setup(x => x.DecodeToken(It.IsAny<string>())).Returns(tokenResponse);
            _userRepository.Setup(x => x.IsExistingUserByEmail(It.IsAny<string>())).ReturnsAsync(false);

            var ex2 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.UserIsInitialPasswordResetCheck(emailToken));
            Assert.That(ex2.Message, Is.EqualTo("USER_NOT_REGISTERED"));

            //Reset link already used
            _jwtUtils.Setup(x => x.IsTokenValid(It.IsAny<string>())).Returns(true);
            _jwtUtils.Setup(x => x.DecodeToken(It.IsAny<string>())).Returns(tokenResponse);
            _userRepository.Setup(x => x.IsExistingUserByEmail(It.IsAny<string>())).ReturnsAsync(true);
            _userRepository.Setup(x => x.IsResetPasswordLinkUsed(It.IsAny<string>())).ReturnsAsync(true);

            var ex3 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.UserIsInitialPasswordResetCheck(emailToken));
            Assert.That(ex3.Message, Is.EqualTo("RESET_PASSWORD_LINK_USED"));

        }

        [Test]
        public async Task UserIsInitialPasswordResetCheck_Success()
        {
            string emailToken = "emailToken";
            TokenResponse tokenResponse = new TokenResponse()
            {
                Email = emailToken,
                Exp = 2,
                Iat = 3,
                Nbf = 4,
            };

            _jwtUtils.Setup(x => x.IsTokenValid(It.IsAny<string>())).Returns(true);
            _jwtUtils.Setup(x => x.DecodeToken(It.IsAny<string>())).Returns(tokenResponse);
            _userRepository.Setup(x => x.IsExistingUserByEmail(It.IsAny<string>())).ReturnsAsync(true);
            _userRepository.Setup(x => x.IsResetPasswordLinkUsed(It.IsAny<string>())).ReturnsAsync(false);
            _userRepository.Setup(x => x.IsInitialPasswordReset(It.IsAny<string>())).ReturnsAsync(true);
            _userRepository.Setup(x => x.IsMigratedAccount(It.IsAny<string>())).ReturnsAsync(true);

            var result = await _authManager.UserIsInitialPasswordResetCheck(emailToken);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Success);
            Assert.That(result.Message, Is.EqualTo("INITIAL_PASSWORD_RESET_CHECK_SUCCESSFUL"));
        }

        [Test]
        public async Task GenerateOTP_Failed()
        {
            string email = "test@gmail.com";
            MpsUser dbUser = new MpsUser()
            {
                FirstName = "Test",
            };

            //User not found
            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((MpsUser)null);

            var ex1 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.GenerateOTP(email));
            Assert.That(ex1.Message, Is.EqualTo("USER_NOT_FOUND"));

            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUser);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync((User)null);

            var ex2 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.GenerateOTP(email));
            Assert.That(ex2.Message, Is.EqualTo("USER_NOT_FOUND"));

        }

        [Test]
        public async Task GenerateOTP_Success()
        {
            string email = "test@gmail.com";
            MpsUser dbUser = new MpsUser()
            {
                FirstName = "Test",
            };

            User user = new User()
            {
                DisplayName = "Test",
            };

            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUser);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync(user);
            _userRepository.Setup(x => x.UpdateUser(It.IsAny<MpsUser>()));

            var result = await _authManager.GenerateOTP(email);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Success);
            Assert.That(result.Message, Is.EqualTo("OTP_GENERATED"));

        }

        [Test]
        public async Task VerifyOtp_Failed()
        {
            string email = "test@gmail.com";
            int otp = 123456;

            MpsUser dbUser = new MpsUser()
            {
                FirstName = "Test",
                OtpIncorrectCount = 6,
            };

            MpsUser dbUserIncorrectOtp = new MpsUser()
            {
                FirstName = "Test",
                OtpIncorrectCount = 1,
                Otp=154152
            };

            MpsUser dbUserExpiredOtp = new MpsUser()
            {
                FirstName = "Test",
                OtpIncorrectCount = 1,
                Otp = 123456,
                OtpGeneratedOn = DateTime.Now.AddMinutes(-6)
            };

            User user = new User()
            {
                DisplayName = "Test",
            };

            //user not found
            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync((MpsUser)null);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync((User)null);

            var ex1 = Assert.ThrowsAsync<UDValiationException>(async() => await _authManager.VerifyOtp(email,otp));
            Assert.That(ex1.Message, Is.EqualTo("USER_NOT_FOUND"));

            //Incorrect otp many times
            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(dbUser);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync(user);

            var ex2 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.VerifyOtp(email, otp));
            Assert.That(ex2.Message, Is.EqualTo("INCORRECT_OTP_MANYTIMES"));

            //Incorrect otp 
            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(dbUserIncorrectOtp);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync(user);
            _userRepository.Setup(x => x.UpdateUser(dbUserIncorrectOtp));

            var ex3 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.VerifyOtp(email, otp));
            Assert.That(ex3.Message, Is.EqualTo("CODE_NOT_WORK"));

            //Expired otp 
            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(dbUserExpiredOtp);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync(user);
            _userRepository.Setup(x => x.UpdateUser(dbUserExpiredOtp));

            var ex4= Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.VerifyOtp(email, otp));
            Assert.That(ex4.Message, Is.EqualTo("CODE_EXPIRED"));
        }

        [Test]
        public async Task VerifyOtp_Success()
        {
            string email = "test@gmail.com";
            int otp = 123456;
            MpsUser dbUserValid = new MpsUser()
            {
                FirstName = "Test",
                OtpIncorrectCount = 1,
                Otp = 123456,
                OtpGeneratedOn = DateTime.Now
            };
            User user = new User()
            {
                DisplayName = "Test",
            };

            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(dbUserValid);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync(user);
            _userRepository.Setup(x => x.UpdateUser(dbUserValid));
            _jwtUtils.Setup(x => x.GenerateToken(It.IsAny<string>(),It.IsAny<int>())).Returns("token");

            var result = await _authManager.VerifyOtp(email, otp);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Success);
            Assert.That(result.Message, Is.EqualTo("OTP_VERIFIED"));
        }

        [Test]
        public async Task GenerateResetPasswordToken_Failed()
        {
            string email = "test@gmail.com";

            MpsUser dbUserInactive = new MpsUser()
            {
                FirstName = "Test",
                IsActive=false
            };
            MpsUser dbUserLocked = new MpsUser()
            {
                FirstName = "Test",
                IsActive=true,
                IsAccountLocked=true
            };
            User user = new User()
            {
                DisplayName = "Test",
            };

            //User not found
            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync((MpsUser)null);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync((User)null);

            var ex1 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.GenerateResetPasswordToken(email));
            Assert.That(ex1.Message, Is.EqualTo("NO_ACCOUNT_WITH_EMAIL"));

            //User not active
            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(dbUserInactive);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync(user);

            var ex2 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.GenerateResetPasswordToken(email));
            Assert.That(ex2.Message, Is.EqualTo("USER_NOT_ACTIVE"));

            //User locked
            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(dbUserLocked);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync(user);

            var ex3 = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.GenerateResetPasswordToken(email));
            Assert.That(ex3.Message, Is.EqualTo("ACCOUNT_LOCKED_DEFAULT"));

        }

        [Test]
        public async Task GenerateResetPasswordToken_Success()
        {
            string email = "test@gmail.com";
            MpsUser dbUserValid = new MpsUser()
            {
                FirstName = "Test",
                IsActive=true,
                IsAccountLocked=false,
            };
            User user = new User()
            {
                DisplayName = "Test",
            };
            UserDto userDto = new UserDto()
            {
                Firstname = "Test",
                Emailaddress = "test@gmail.com",
                Otp = 123456,
                Lastname = "Test",
                Languagecode = "en",
            };
            UserResponseDto userResponseDto = new UserResponseDto()
            {
                Firstname = "Test",
                Emailaddress = "test@gmail.com"
            };

            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(dbUserValid);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync(user);
            _jwtUtils.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<int>())).Returns("token");
            _userRepository.Setup(x => x.UpdateUser(dbUserValid)).ReturnsAsync(userDto);
            _mapper.Setup(x => x.Map<UserResponseDto>(userDto)).Returns(userResponseDto);

            var result = await _authManager.GenerateResetPasswordToken(email);

            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.IsTrue(result.Success);
            Assert.That(result.Message, Is.EqualTo("TOKEN_GENERATED"));
        }

        [Test]
        public async Task GenerateAccessToken_UserNotFound_ThrowsException()
        {
            string email = "test@gmail.com";
            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync((MpsUser)null);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync((User)null);

            var ex = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.GenerateAccessToken(email));
            Assert.That(ex.Message, Is.EqualTo("NO_ACCOUNT_WITH_EMAIL"));
        }

        [Test]
        public async Task GenerateAccessToken_UserNotActive_ThrowsException()
        {
            string email = "test@gmail.com";
            var dbUser = new MpsUser { IsActive = false };
            var user = new User();

            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(dbUser);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync(user);

            var ex = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.GenerateAccessToken(email));
            Assert.That(ex.Message, Is.EqualTo("USER_NOT_ACTIVE"));
        }

        [Test]
        public async Task GenerateAccessToken_UserAccountLocked_ThrowsException()
        {
            string email = "test@gmail.com";
            var dbUser = new MpsUser { IsActive = true, IsAccountLocked = true };
            var user = new User();

            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(dbUser);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync(user);

            var ex = Assert.ThrowsAsync<UDValiationException>(async () => await _authManager.GenerateAccessToken(email));
            Assert.That(ex.Message, Is.EqualTo("ACCOUNT_LOCKED_DEFAULT"));
        }

        public async Task GenerateAccessToken_Success()
        {
            string email = "test@gmail.com";
            MpsUser dbUserValid = new MpsUser()
            {
                FirstName = "Test",
                IsActive = true,
                IsAccountLocked = false,
            };
            User user = new User()
            {
                DisplayName = "Test",
            };
            UserDto userDto = new UserDto()
            {
                Firstname = "Test",
                Emailaddress = "test@gmail.com",
                Otp = 123456,
                Lastname = "Test",
                Languagecode = "en",
            };
            UserResponseDto userResponseDto = new UserResponseDto()
            {
                Firstname = "Test",
                Emailaddress = "test@gmail.com"
            };

            _userRepository.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(dbUserValid);
            _externalUserService.Setup(x => x.GetUserBySignInName(email, Core.Enums.UserType.Client)).ReturnsAsync(user);
            _jwtUtils.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<int>())).Returns("token");
            _userRepository.Setup(x => x.UpdateUser(dbUserValid)).ReturnsAsync(userDto);
            _mapper.Setup(x => x.Map<UserResponseDto>(userDto)).Returns(userResponseDto);

            var result = await _authManager.GenerateAccessToken(email);

            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.IsTrue(result.Success);
            Assert.That(result.Message, Is.EqualTo("ACCESS_TOKEN_GENERATED_SUCCESSFULLY"));
        }

    }
}


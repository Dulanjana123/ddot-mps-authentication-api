using AutoMapper;
using Core.CoreSettings;
using Core.Utilities;
using DataAccess.Entities;
using DataAccess.Repositories;
using DDOT.MPS.Auth.Api.Managers;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Model.Dtos;
using Model.Request.Generic;
using Model.Request;
using Model.Response;
using Model.Services.UserManagement;
using Moq;
using static Core.Exceptions.UserDefinedException;
using Test.Helpers;
using Microsoft.Extensions.Logging;

namespace Test.Managers
{

    [TestFixture]
    public class UserManagerTest
    {
        private Mock<IMpsUserRepository> _userRepository;
        private Mock<IExternalUserService> _externalUserService;
        private Mock<IMapper> _mapper;
        private Mock<IAppUtils> _appUtils;
        private Mock<IJwtUtils> _jwtUtils;
        private IOptions<GlobalAppSettings> _globalAppSettings;
        private IUserManager _userManager;
        private Mock<ILogger<UserManager>> _logger;
        private Mock<IMpsAgencyRepository> _agencyRepository;

        [SetUp]
        public void SetUp()
        {
            _userRepository = new Mock<IMpsUserRepository>();
            _externalUserService = new Mock<IExternalUserService>();
            _mapper = new Mock<IMapper>();
            _appUtils = new Mock<IAppUtils>();
            _jwtUtils = new Mock<IJwtUtils>();
            _globalAppSettings = Options.Create(new GlobalAppSettings { OtpExpirationTime = 5, MaxOtpIncorrectCount = 3 });
            _logger = new Mock<ILogger<UserManager>>();
            _agencyRepository = new Mock<IMpsAgencyRepository>();
            _userManager = new UserManager(_userRepository.Object, _externalUserService.Object, _mapper.Object, _globalAppSettings, _appUtils.Object, _jwtUtils.Object, _logger.Object, _agencyRepository.Object);
        }

        [Test]
        public async Task CreateUser_EmailAlreadyRegistered_ThrowsException()
        {
            var userDto = new UserDto { Emailaddress = "test@example.com" };
            var user = new User
            {
                DisplayName = "Lakshan"
            };
            _externalUserService.Setup(x => x.GetUserBySignInName(userDto.Emailaddress, Core.Enums.UserType.Client)).ReturnsAsync(user);

            var ex = Assert.ThrowsAsync<UDValiationException>(() => _userManager.CreateUser(userDto));
            Assert.That(ex.Message, Is.EqualTo("EMAIL_ALREADY_USED"));
        }

        [Test]
        public async Task CreateUser_WeakPassword_ThrowsException()
        {
            var userDto = new UserDto { Emailaddress = "test@example.com", Password = "weakpass" };
            _userRepository.Setup(x => x.IsExistingUserByEmail(userDto.Emailaddress)).ReturnsAsync(false);
            _appUtils.Setup(x => x.IsValidPassword(userDto.Password)).Returns(false);

            var ex = Assert.ThrowsAsync<UDValiationException>(() => _userManager.CreateUser(userDto));
            Assert.That(ex.Message, Is.EqualTo("WEAK_PASSWORD"));
        }

        [Test]
        public async Task CreateUser_InvalidPhoneNumber_ThrowsException()
        {
            var userDto = new UserDto { Emailaddress = "test@example.com", Password = "StrongPass1!", Mobilenumber = "12345" };
            _userRepository.Setup(x => x.IsExistingUserByEmail(userDto.Emailaddress)).ReturnsAsync(false);
            _appUtils.Setup(x => x.IsValidPassword(userDto.Password)).Returns(true);
            _appUtils.Setup(x => x.IsValidPhoneNumber(userDto.Mobilenumber)).Returns(false);

            var ex = Assert.ThrowsAsync<UDValiationException>(() => _userManager.CreateUser(userDto));
            Assert.That(ex.Message, Is.EqualTo("MOBILE_INVALID"));
        }

        [Test]
        public async Task CreateUser_ErrorCreatingUserInB2CTenant_ThrowsException()
        {
            var userDto = new UserDto { Emailaddress = "test@example.com", Password = "StrongPass1!", Mobilenumber = "1234567890", AgencyId = 2, UserTypeId = 2 };
            _userRepository.Setup(x => x.IsExistingUserByEmail(userDto.Emailaddress)).ReturnsAsync(false);
            _appUtils.Setup(x => x.IsValidPassword(userDto.Password)).Returns(true);
            _appUtils.Setup(x => x.IsValidPhoneNumber(userDto.Mobilenumber)).Returns(true);
            _externalUserService.Setup(x => x.CreateNewUser(It.IsAny<UserRegistrationB2cDto>(), Core.Enums.UserType.Client)).ReturnsAsync((User)null);

            var ex = Assert.ThrowsAsync<UDValiationException>(() => _userManager.CreateUser(userDto));
            Assert.That(ex.Message, Is.EqualTo("Error occurred while creating user in B2C Tenant."));
        }

        [Test]
        public async Task CreateUser_Successful_ReturnsUserResponse()
        {
            var userDto = new UserDto { Emailaddress = "test@example.com", Password = "StrongPass1!", Mobilenumber = "1234567890", AgencyId = 2, UserTypeId = 2 };
            var createdUser = new User();
            var mpsUser = new MpsUser();
            var userResponseDto = new UserResponseDto();

            _userRepository.Setup(x => x.IsExistingUserByEmail(userDto.Emailaddress)).ReturnsAsync(false);
            _appUtils.Setup(x => x.IsValidPassword(userDto.Password)).Returns(true);
            _appUtils.Setup(x => x.IsValidPhoneNumber(userDto.Mobilenumber)).Returns(true);
            _externalUserService.Setup(x => x.CreateNewUser(It.IsAny<UserRegistrationB2cDto>(), Core.Enums.UserType.Client)).ReturnsAsync(createdUser);
            _mapper.Setup(m => m.Map<MpsUser>(userDto)).Returns(mpsUser);
            _mapper.Setup(m => m.Map<UserResponseDto>(mpsUser)).Returns(userResponseDto);

            var result = await _userManager.CreateUser(userDto);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(userResponseDto, result.Data);
        }

        [Test]
        public async Task ValidateOTP_IncorrectOTP_ThrowsException()
        {
            var user = new MpsUser { Otp = 654321, OtpIncorrectCount = 0 };
            _userRepository.Setup(x => x.GetUserById(It.IsAny<int>())).ReturnsAsync(user);

            var ex = Assert.ThrowsAsync<UDValiationException>(() => _userManager.ValidateOTP(1, 123456));
            Assert.That(ex.Message, Is.EqualTo("USER_NOT_FOUND"));
        }

        [Test]
        public async Task ValidateOTP_ExpiredOTP_ThrowsException()
        {
            var user = new MpsUser { Otp = 123456, OtpGeneratedOn = DateTime.Now.AddMinutes(-10) };
            _userRepository.Setup(x => x.GetUserById(It.IsAny<int>())).ReturnsAsync(user);

            var ex = Assert.ThrowsAsync<UDValiationException>(() => _userManager.ValidateOTP(1, 123456));
            Assert.That(ex.Message, Is.EqualTo("USER_NOT_FOUND"));
        }

        [Test]
        public async Task ValidateOTP_Successful_ReturnsUserResponse()
        {
            var user = new MpsUser { Otp = 123456, OtpGeneratedOn = DateTime.Now, OtpIncorrectCount = 0 };
            var userResponseDto = new UserResponseDto();

            _userRepository.Setup(x => x.GetUserById(It.IsAny<int>())).ReturnsAsync(user);
            _externalUserService.Setup(x => x.GetUserBySignInName(user.EmailAddress, Core.Enums.UserType.Client)).ReturnsAsync(new User());
            _mapper.Setup(m => m.Map<UserResponseDto>(user)).Returns(userResponseDto);

            var result = await _userManager.ValidateOTP(1, 123456);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(userResponseDto, result.Data);
        }

        [Test]
        public async Task CreateAdmin_EmailAlreadyInUse_ThrowsException()
        {
            var userDto = new UserDto { Emailaddress = "test123@example.com", Password = "pass", UserType = Core.Enums.UserType.Admin };
            var user = new User
            {
                DisplayName = "Shamika"
            };
            _externalUserService.Setup(x => x.GetUserBySignInName(userDto.Emailaddress, Core.Enums.UserType.Admin)).ReturnsAsync(user);

            var ex = Assert.ThrowsAsync<UDValiationException>(() => _userManager.CreateUser(userDto));
            Assert.That(ex.Message, Is.EqualTo("EMAIL_ALREADY_USED"));
        }

        [Test]
        public async Task CreateAdmin_InvalidPhoneNumber_ThrowsException()
        {
            var userDto = new UserDto { Emailaddress = "test@example.com", Password = "StrongPass1!", Mobilenumber = "12345", UserType = Core.Enums.UserType.Admin };
            _userRepository.Setup(x => x.IsExistingUserByEmail(userDto.Emailaddress)).ReturnsAsync(false);
            _appUtils.Setup(x => x.IsValidPassword(userDto.Password)).Returns(true);
            _appUtils.Setup(x => x.IsValidPhoneNumber(userDto.Mobilenumber)).Returns(false);

            var ex = Assert.ThrowsAsync<UDValiationException>(() => _userManager.CreateUser(userDto));
            Assert.That(ex.Message, Is.EqualTo("MOBILE_INVALID"));
        }

        [Test]
        public async Task CreateAdmin_ErrorCreatingUserInB2CTenant_ThrowsException()
        {
            var userDto = new UserDto { Emailaddress = "test@example.com", Password = "StrongPass1!", Mobilenumber = "1234567890", UserType = Core.Enums.UserType.Admin, AgencyId = 2, UserTypeId = 2 };
            _userRepository.Setup(x => x.IsExistingUserByEmail(userDto.Emailaddress)).ReturnsAsync(false);
            _appUtils.Setup(x => x.IsValidPassword(userDto.Password)).Returns(true);
            _appUtils.Setup(x => x.IsValidPhoneNumber(userDto.Mobilenumber)).Returns(true);
            _externalUserService.Setup(x => x.CreateNewUser(It.IsAny<UserRegistrationB2cDto>(), Core.Enums.UserType.Admin)).ReturnsAsync((User)null);

            var ex = Assert.ThrowsAsync<UDValiationException>(() => _userManager.CreateUser(userDto));
            Assert.That(ex.Message, Is.EqualTo("Error occurred while creating user in B2C Tenant."));
        }

        [Test]
        public async Task CreateAdmin_Successful_ReturnsUserResponse()
        {
            var userDto = new UserDto { Emailaddress = "test@example.com", Password = "StrongPass1!", Mobilenumber = "1234567890", UserType = Core.Enums.UserType.Admin, AgencyId = 2, UserTypeId = 2 };
            var createdUser = new User();
            var mpsUser = new MpsUser();
            var userResponseDto = new UserResponseDto();

            _userRepository.Setup(x => x.IsExistingUserByEmail(userDto.Emailaddress)).ReturnsAsync(false);
            _appUtils.Setup(x => x.IsValidPassword(userDto.Password)).Returns(true);
            _appUtils.Setup(x => x.IsValidPhoneNumber(userDto.Mobilenumber)).Returns(true);
            _externalUserService.Setup(x => x.CreateNewUser(It.IsAny<UserRegistrationB2cDto>(), Core.Enums.UserType.Admin)).ReturnsAsync(createdUser);
            _mapper.Setup(m => m.Map<MpsUser>(userDto)).Returns(mpsUser);
            _mapper.Setup(m => m.Map<UserResponseDto>(mpsUser)).Returns(userResponseDto);

            var result = await _userManager.CreateUser(userDto);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(userResponseDto, result.Data);
        }

        [Test]
        public async Task GetPaginatedList_ValidRequest_ReturnsPaginatedResponse()
        {
            UserPaginatedRequest request = new UserPaginatedRequest
            {
                PagingAndSortingInfo = new PagingAndSortingInfo
                {
                    Paging = new PagingInfo
                    {
                        PageSize = 10,
                        PageNo = 1
                    }
                }
            };

            IQueryable<MpsUser> mpsUsers = new List<MpsUser> { new MpsUser(), new MpsUser() }.AsQueryable();
            List<UserDetailsDto> userResponseDtos = new List<UserDetailsDto> { new UserDetailsDto(), new UserDetailsDto() };

            TestAsyncEnumerable<MpsUser> mockUserQueryable = new TestAsyncEnumerable<MpsUser>(mpsUsers);

            _userRepository.Setup(x => x.GetAll(It.IsAny<UserPaginatedRequest>()))
                           .Returns(mockUserQueryable);

            _userRepository.Setup(x => x.GetRowCount(It.IsAny<UserPaginatedRequest>()))
                           .Returns(mpsUsers.Count());

            _mapper.Setup(m => m.Map<UserDetailsDto>(It.IsAny<MpsUser>()))
                   .Returns(new UserDetailsDto());

            BaseResponse<Result<UserDetailsDto>> result = await _userManager.GetPaginatedList(request);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(userResponseDtos.Count, result.Data.Entities.Length);
            Assert.AreEqual(mpsUsers.Count(), result.Data.Pagination.Length);
            Assert.AreEqual(request.PagingAndSortingInfo.Paging.PageSize, result.Data.Pagination.PageSize);
        }

        [Test]
        public async Task GetPaginatedList_EmptyData_ReturnsEmptyResponse()
        {
            UserPaginatedRequest request = new UserPaginatedRequest
            {
                PagingAndSortingInfo = new PagingAndSortingInfo
                {
                    Paging = new PagingInfo
                    {
                        PageSize = 10,
                        PageNo = 1
                    }
                }
            };

            IQueryable<MpsUser> mpsUsers = new List<MpsUser>().AsQueryable();
            TestAsyncEnumerable<MpsUser> mockUserQueryable = new TestAsyncEnumerable<MpsUser>(mpsUsers);

            _userRepository.Setup(x => x.GetAll(It.IsAny<UserPaginatedRequest>()))
                           .Returns(mockUserQueryable);

            _userRepository.Setup(x => x.GetRowCount(It.IsAny<UserPaginatedRequest>()))
                           .Returns(mpsUsers.Count());

            BaseResponse<Result<UserDetailsDto>> result = await _userManager.GetPaginatedList(request);

            Assert.IsTrue(result.Success);
            Assert.IsEmpty(result.Data.Entities);
            Assert.AreEqual(0, result.Data.Pagination.Length);
        }
    }
}

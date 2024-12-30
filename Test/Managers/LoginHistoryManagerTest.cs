using AutoMapper;
using Core.Enums;
using DataAccess.Entities;
using DataAccess.Repositories;
using DDOT.MPS.Auth.Api.Managers;
using Microsoft.Graph.Models;
using Model.Dtos;
using Model.Services.UserManagement;
using Moq;
using static Core.Exceptions.UserDefinedException;

namespace Test.Managers
{

    [TestFixture]
    public class LoginHistoryManagerTest
    {
        private ILoginHistoryManager _loginHistoryManager;
        private Mock<IMpsUserRepository> _userRepository;
        private Mock<IExternalUserService> _externalUserService;
        private Mock<IMapper> _mapper;
        private Mock<IMpsLoginHistoryRepository> _loginHistoryRepository;

        [SetUp]
        public void SetUp()
        {
            _userRepository = new Mock<IMpsUserRepository>();
            _externalUserService = new Mock<IExternalUserService>();
            _mapper = new Mock<IMapper>();
            _loginHistoryRepository = new Mock<IMpsLoginHistoryRepository>();
            _loginHistoryManager = new LoginHistoryManager(_mapper.Object, _userRepository.Object, _externalUserService.Object, _loginHistoryRepository.Object);
        }

        [Test]
        public async Task LoginHistory_UserNotFound_Db()
        {
            LoginHistoryBaseDto loginHistoryBaseDto = new LoginHistoryBaseDto
            {
                Timestamp = DateTime.Now,
                UserEmail = "test@gmail.com",
                Userintractionid = UserIntractionType.Login
            };

            LoginHistoryHeaderDto loginHistoryHeaderDto = new LoginHistoryHeaderDto
            {
                Agent = "test",
                IpAddress = "192.168.2.5"
            };

            User user = new User
            {
                DisplayName = "Lakshan"
            };

            _userRepository.Setup(x => x.GetUserByEmail(loginHistoryBaseDto.UserEmail)).ReturnsAsync((MpsUser)null);
            _externalUserService.Setup(x => x.GetUserBySignInName(loginHistoryBaseDto.UserEmail, Core.Enums.UserType.Client)).ReturnsAsync(user);

            var ex = Assert.ThrowsAsync<UDValiationException>(async () => await _loginHistoryManager.CreateLoginHistory(loginHistoryBaseDto, loginHistoryHeaderDto));
            Assert.That(ex.Message, Is.EqualTo("NO_ACCOUNT_WITH_EMAIL"));
        }

        [Test]
        public async Task LoginHistory_UserNotFound_B2C()
        {
            LoginHistoryBaseDto loginHistoryBaseDto = new LoginHistoryBaseDto
            {
                Timestamp = DateTime.Now,
                UserEmail = "test@gmail.com",
                Userintractionid = UserIntractionType.Login
            };

            LoginHistoryHeaderDto loginHistoryHeaderDto = new LoginHistoryHeaderDto
            {
                Agent = "test",
                IpAddress = "192.168.2.5"
            };

            MpsUser user = new MpsUser
            {
                FirstName = "Test",
            };

            _userRepository.Setup(x => x.GetUserByEmail(loginHistoryBaseDto.UserEmail)).ReturnsAsync(user);
            _externalUserService.Setup(x => x.GetUserBySignInName(loginHistoryBaseDto.UserEmail, Core.Enums.UserType.Client)).ReturnsAsync((User)null);

            var ex = Assert.ThrowsAsync<UDValiationException>(async () => await _loginHistoryManager.CreateLoginHistory(loginHistoryBaseDto, loginHistoryHeaderDto));
            Assert.That(ex.Message, Is.EqualTo("NO_ACCOUNT_WITH_EMAIL"));
        }

        [Test]
        public async Task LoginHistory_Create_Success()
        {
            LoginHistoryBaseDto loginHistoryBaseDto = new LoginHistoryBaseDto
            {
                Timestamp = DateTime.Now,
                UserEmail = "test@gmail.com",
                Userintractionid = UserIntractionType.Login
            };

            LoginHistoryHeaderDto loginHistoryHeaderDto = new LoginHistoryHeaderDto
            {
                Agent = "test",
                IpAddress = "192.168.2.5"
            };

            MpsUser dbUser = new MpsUser
            {
                FirstName = "Test",
            };

            User b2cUser = new User
            {
                DisplayName = "Lakshan"
            };

            FullLoginHistoryDto fullLoginHistoryDto = new FullLoginHistoryDto
            {
                Browser = "test",
                Detaileddescription = "test",
                Device = "test",
                IpAddress = "12",
                Os = "test",
                Timestamp = DateTime.Now,
                UserId = 1,
                Userintractionid = UserIntractionType.Login
            };

            _userRepository.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(dbUser);
            _externalUserService.Setup(x => x.GetUserBySignInName(It.IsAny<string>(), Core.Enums.UserType.Client)).ReturnsAsync(b2cUser);
            _loginHistoryRepository.Setup(x => x.CreateLoginHistory(It.IsAny<FullLoginHistoryDto>())).ReturnsAsync(new FullLoginHistoryDto());

            var result = await _loginHistoryManager.CreateLoginHistory(loginHistoryBaseDto, loginHistoryHeaderDto);
            Assert.NotNull(result.Data);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("LOGIN_HISTORY_CREATED_SUCCESSFULLY", result.Message);
        }

    }
}


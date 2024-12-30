using Core.CommonDtos;
using Core.CoreSettings;
using Core.Utilities;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace Test.Utilities
{
    [TestFixture]
    public class JwtUtilsTest
    {
        private IJwtUtils _jwtUtils;
        private Mock<IOptions<GlobalAppSettings>> _mockOptions;
        private GlobalAppSettings _globalAppSettings;

        [SetUp]
        public void SetUp()
        {
            _globalAppSettings = new GlobalAppSettings
            {
                JwtSecretKey = "eyJhbGciOiJIUzI1NiJ9.eyJSb2xlIjoiQWRt"
            };
            _mockOptions = new Mock<IOptions<GlobalAppSettings>>();
            _mockOptions.Setup(x => x.Value).Returns(_globalAppSettings);
            _jwtUtils = new JwtUtils(_mockOptions.Object);
        }

        [Test]
        public void GenerateToken_ShouldReturnValidToken()
        {
            string email = "test@example.com";
            int expiryHours = 1;

            string token = _jwtUtils.GenerateToken(email, expiryHours);

            Assert.IsNotNull(token);
            Assert.IsTrue(_jwtUtils.IsTokenValid(token));
        }

        [Test]
        public void IsTokenValid_ShouldReturnTrueForValidToken()
        {
            string email = "test@example.com";
            string token = _jwtUtils.GenerateToken(email, 1);

            bool isValid = _jwtUtils.IsTokenValid(token);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsTokenValid_ShouldReturnFalseForInvalidToken()
        {
            string token = "invalid.token.value";

            bool isValid = _jwtUtils.IsTokenValid(token);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void DecodeToken_ShouldReturnCorrectTokenResponse()
        {
            string email = "test@example.com";
            string token = _jwtUtils.GenerateToken(email, 1);

            TokenResponse tokenResponse = _jwtUtils.DecodeToken(token);

            Assert.That(email,Is.EqualTo(tokenResponse.Email));
            Assert.Greater(tokenResponse.Nbf, 0);
            Assert.Greater(tokenResponse.Exp, 0);
            Assert.Greater(tokenResponse.Iat, 0);
        }

        [Test]
        public void DecodeToken_ShouldThrowArgumentExceptionForInvalidToken()
        {
            string token = "invalid.token.value";

            Assert.Throws<ArgumentException>(() => _jwtUtils.DecodeToken(token), "TOKEN_INVALID");
        }

        [Test]
        public void GenerateTokenV2_ShouldReturnValidToken()
        {
            var user = new AuthUserDto
            {
                Emailaddress = "test@example.com",
                Firstname = "John",
                Lastname = "Doe",
                RoleId = 1,
                Mobilenumber = "1234567890",
                Userid = 3
            };
            int expiryHours = 1;

            string token = _jwtUtils.GenerateToken(user, expiryHours);

            Assert.IsNotNull(token);
            Assert.IsTrue(_jwtUtils.IsTokenValid(token));
        }

        [Test]
        public void GenerateTokenV2_ShouldContainCorrectClaims()
        {
            var user = new AuthUserDto
            {
                Emailaddress = "test@example.com",
                Firstname = "John",
                Lastname = "Doe",
                RoleId = 1,
                Mobilenumber = "1234567890",
                Userid = 3
            };
            int expiryHours = 1;

            string token = _jwtUtils.GenerateToken(user, expiryHours);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.AreEqual(user.Emailaddress, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
            Assert.AreEqual(user.Firstname, jwtToken.Claims.First(c => c.Type == "FirstName").Value);
            Assert.AreEqual(user.Lastname, jwtToken.Claims.First(c => c.Type == "LastName").Value);
            Assert.AreEqual(user.RoleId.ToString(), jwtToken.Claims.First(c => c.Type == "RoleId").Value);
            Assert.AreEqual(user.Mobilenumber, jwtToken.Claims.First(c => c.Type == "MobileNumber").Value);
            Assert.AreEqual(user.Userid.ToString(), jwtToken.Claims.First(c => c.Type == "UserId").Value);
        }
    }
}

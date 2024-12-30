using Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Utilities
{
    [TestFixture]
    public class AppUtilsTest
    {
        private IAppUtils _appUtils;

        [SetUp]
        public void SetUp()
        {
            _appUtils = new AppUtils();
        }

        [Test]
        [TestCase("Password1!", true)]
        [TestCase("password1!", false)]
        [TestCase("PASSWORD1!", false)]
        [TestCase("Password!", false)]
        [TestCase("Password1", false)]
        [TestCase("Pass1!", false)]
        [TestCase("", false)]
        public void IsValidPassword_ShouldReturnExpectedResult(string password, bool expectedResult)
        {
            bool result = _appUtils.IsValidPassword(password);

            Assert.That(expectedResult, Is.EqualTo(result));
        }

        [Test]
        [TestCase("+1234567890", true)]
        [TestCase("123-456-7890", false)]
        [TestCase("+12 345 678 901", false)]
        [TestCase("", false)]
        public void IsValidPhoneNumber_ShouldReturnExpectedResult(string phoneNumber, bool expectedResult)
        {
            bool result = _appUtils.IsValidPhoneNumber(phoneNumber);

            Assert.That(expectedResult, Is.EqualTo(result));
        }
    }
}

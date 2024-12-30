using Core.CommonDtos;
using Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Test.Utilities
{
    [TestFixture]
    public class AuthUtilsTest
    {
        [Test]
        public void Generate6DigitOTP_ShouldReturnSixDigitNumber()
        {
            int otp = AuthUtils.Generate6DigitOTP();

            Assert.That(otp, Is.GreaterThanOrEqualTo(100000).And.LessThanOrEqualTo(999999), "OTP is not a 6-digit number");
        }

        [Test]
        public void GetDeviceInfo_ShouldReturnCorrectDeviceInfo()
        {
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
            var expectedBrowser = "Chrome";
            var expectedDevice = "Other";
            var expectedOS = "Windows";

            LoginHistoryRequestInfo deviceInfo = AuthUtils.GetDeviceInfo(userAgent);

            Assert.That(expectedBrowser, Is.EqualTo(deviceInfo.Browser), "Browser does not match");
            Assert.That(expectedDevice, Is.EqualTo(deviceInfo.Device), "Device does not match");
            Assert.That(expectedOS, Is.EqualTo(deviceInfo.Os), "OS does not match");
            Assert.IsTrue(Regex.IsMatch(deviceInfo.Detaileddescription, "Chrome/91.0.4472.124"), "Detailed description does not match");
        }
    }
}

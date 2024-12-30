using Core.CommonDtos;

namespace Core.Utilities
{
    public class AuthUtils
    {
        /// <summary>
        /// Generates a 6-digit OTP and returns it as a int data type.
        /// </summary>
        /// <returns>A 6-digit OTP as a int data type.</returns>
        public static int Generate6DigitOTP()
        {
            int otp = new Random().Next(100000, 1000000);
            return Math.Abs(otp); ;
        }

        public static LoginHistoryRequestInfo GetDeviceInfo(string userAgent)
        {
            UAParser.Parser uaParser = UAParser.Parser.GetDefault();
            UAParser.Objects.ClientInfo clientInfo = uaParser.Parse(userAgent);
            return new LoginHistoryRequestInfo
            {
                Browser = clientInfo.Browser.Family,
                Device = clientInfo.Device.Family,
                Os = clientInfo.OS.Family,
                Detaileddescription = clientInfo.String
            };
        }
    }
}

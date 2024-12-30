namespace Core.CoreSettings
{
    public class GlobalAppSettings
    {
        public int OtpExpirationTime { get; set; }
        public int MaxOtpIncorrectCount { get; set; }
        public string JwtSecretKey { get; set; }
        public int ResetPasswordTokenExpiryHours { get; set; }
        public int LoginTokenExpiryHours { get; set; }
    }
}

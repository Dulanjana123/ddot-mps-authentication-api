namespace Model.Dtos
{
    public class ValidateOtpDto
    {
        public int UserId { get; set; }
        public int Otp { get; set; }
    }

    public class OtpGenerateDto
    {
        public string Email { get; set; }
        public int Otp { get; set; }
    }

    public class OtpGenerateResponseDto
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string? Languagecode { get; set; }
        public int Otp { get; set; }
        public string? AccessToken { get; set; }
    }

    public class OtpVerifyResponseDto
    {
        public string Email { get; set; }
        public string? Token { get; set; }
        public bool Verified { get; set; } = false;
    }
}

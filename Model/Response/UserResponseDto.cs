namespace Model.Response
{    public  class UserResponseDto
    {
        public int Userid { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Emailaddress { get; set; }
        public string? Languagecode { get; set; }
        public string Mobilenumber { get; set; }
        public string? Token { get; set; }
        public int Otp { get; set; }
    }

    public class UserTokenResponse
    {
        public string Token { get; set; }
        public UserResponseDto User { get; set; }
    }
}

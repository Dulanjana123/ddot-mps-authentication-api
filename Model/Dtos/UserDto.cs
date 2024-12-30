using Core.Enums;

namespace Model.Dtos
{
    public class UserDto
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Emailaddress { get; set; }
        public string? Languagecode { get; set; }
        public string Password { get; set; }
        public string Mobilenumber { get; set; }
        public int? Otp { get; set; }
        public DateTime? Otpgeneratedon { get; set; }
        public UserType? UserType { get; set; } = Core.Enums.UserType.Client;
        public int UserTypeId { get; set; }
        public int? AgencyId { get; set; }
    }
}

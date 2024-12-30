namespace Core.CommonDtos
{
    public class AuthUserDto
    {
        public int Userid { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Emailaddress { get; set; }
        public int? RoleId { get; set; }
        public string Mobilenumber { get; set; }
    }
}

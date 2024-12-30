namespace Model.Response
{
    public class UserDetailsDto
    {
        public int Userid { get; set; }

        public string Emailaddress { get; set; } = null!;

        public string? Firstname { get; set; }

        public string? Lastname { get; set; }

        public string? Mobilenumber { get; set; }

        public bool Isactive { get; set; }
    }
}

namespace Model.Dtos
{
    public class ResetPasswordDto
    {
        public string Emailaddress { get; set; }
        public string Password { get; set; }
        public string? Mobilenumber { get; set; }
    }
}

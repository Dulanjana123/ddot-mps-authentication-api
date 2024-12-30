namespace Model.Response
{
    public class ResetPasswordResponseDto
    {
        public UserResponseDto User { get; set; }
        public B2CTokenResponse LoginResponse { get; set; }
    }
}

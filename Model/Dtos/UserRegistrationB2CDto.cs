using Newtonsoft.Json;

namespace Model.Dtos
{
    public class UserRegistrationB2cDto
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string Password { get; set; }
    }
    public class UserLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class User2FADto
    {
        public string Code { get; set; }
    }
}
public class B2CTokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
    [JsonProperty("token_type")]
    public string token_type { get; set; }
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
    [JsonProperty("id_token")]
    public string  IdTokenType { get; set; }
    public string? LoginToken { get; set; }
    public string Email { get; set; }
}

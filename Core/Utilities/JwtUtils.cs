using Core.CommonDtos;
using Core.CoreSettings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Core.Utilities
{
    public class TokenResponse
    {
        public string Email { get; set; }
        public long Nbf { get; set; }
        public long Exp { get; set; }
        public long Iat { get; set; }
    }

    public interface IJwtUtils
    {
        string GenerateToken(string email, int expiryHours);
        public string GenerateToken(AuthUserDto user, int expiryHours);
        bool IsTokenValid(string token);
        TokenResponse DecodeToken(string token);
    }

    public class JwtUtils : IJwtUtils
    {
        private readonly SymmetricSecurityKey _signingKey;
        private readonly GlobalAppSettings _globalAppSettings;

        public JwtUtils(IOptions<GlobalAppSettings> globalAppSettings)
        {
            _globalAppSettings = globalAppSettings.Value;
            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_globalAppSettings.JwtSecretKey));
        }
        public TokenResponse DecodeToken(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            if (tokenHandler.CanReadToken(token))
            {
                JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token);
                IEnumerable<Claim> claims = jwtToken.Claims;
                
                TokenResponse tokenResponse = new TokenResponse();
                Dictionary<string,object> claimDict = new Dictionary<string, object>();
                foreach (Claim claim in claims)
                {
                    switch(claim.Type)
                    {
                        case JwtRegisteredClaimNames.Email:
                            tokenResponse.Email = claim.Value;
                            break;
                        case JwtRegisteredClaimNames.Nbf:
                            tokenResponse.Nbf = long.Parse(claim.Value);
                            break;
                        case JwtRegisteredClaimNames.Exp:
                            tokenResponse.Exp = long.Parse(claim.Value);
                            break;
                        case JwtRegisteredClaimNames.Iat:
                            tokenResponse.Iat = long.Parse(claim.Value);
                            break;
                        default:
                            break;
                    }
                }

                return tokenResponse;
            }
            else
            {
                throw new ArgumentException("TOKEN_INVALID");   
            }
        }

        public string GenerateToken(AuthUserDto user, int expiryHours = 24)
        {

            Claim[] claims = ([
                new Claim(JwtRegisteredClaimNames.Email, user.Emailaddress),
                new Claim("FirstName", user.Firstname),
                new Claim("LastName", user.Lastname),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim("MobileNumber", user.Mobilenumber),
                new Claim("UserId", user.Userid.ToString())
                ]);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
                {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expiryHours),
                SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);

        }

        public string GenerateToken(string email, int expiryHours = 24)
        {
            Claim[] claims = ([
                new Claim(JwtRegisteredClaimNames.Email, email)
                ]);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expiryHours),
                SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        public bool IsTokenValid(string token)
        {
            // Configure validation parameters
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = _signingKey,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true
            };
            // Try to validate the token
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception)
            { 
                return false;
            }
        }
    }
}

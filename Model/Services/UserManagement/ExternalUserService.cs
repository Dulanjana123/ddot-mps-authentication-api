using Core.Enums;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Authentication.Methods.Item.ResetPassword;
using Microsoft.Identity.Client;
using Model.Dtos;
using Model.OptionModels;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static Core.Exceptions.UserDefinedException;

namespace Model.Services.UserManagement
{
    public class ExternalUserService : IExternalUserService
    {
        private readonly IGraphApiAuthorizationCodeProvider _provider;
        private readonly GraphApiCredentialOptionsForClient _clientOptions;
        private readonly GraphApiCredentialOptionsForAdmin _adminOptions;
        private readonly HttpClient _httpClient;
        public ExternalUserService(IGraphApiAuthorizationCodeProvider provider, IOptions<GraphApiCredentialOptionsForClient> clientOptions, IOptions<GraphApiCredentialOptionsForAdmin> adminOptions, HttpClient httpClient)
        {
            _provider = provider;
            _clientOptions = clientOptions.Value;
            _adminOptions = adminOptions.Value;
            _httpClient = httpClient;
        }
        private GraphServiceClient GetServiceClient(UserType userType = UserType.Client)
        {
            var authProvider = _provider.GetAuthProvider(userType);
            return new GraphServiceClient(authProvider);
        }

        private async Task<User> GetUserByEmailAddress(string email, GraphServiceClient client)
        {
            var filterQuery = $"mail eq '{email}'";
            var usersPage = await client.Users
                                        .GetAsync(requestConfig => requestConfig.QueryParameters.Filter = filterQuery);

            return usersPage.Value.FirstOrDefault();
        }

        public async Task<User> CreateNewUser(UserRegistrationB2cDto user, UserType userType = UserType.Client)
        {
            var client = GetServiceClient(userType);
            var userCheck = await GetUserByEmailAddress(user.Email, client);
            if (userCheck != null)
            {
                throw new ValidationException("USER_WITH_EMAIL_EXIST");
            }

            User graphUser;

            if (userType.Equals(UserType.Client))
            {
                graphUser = new User()
                {
                    GivenName = user.FirstName,
                    Surname = user.LastName,
                    Mail = user.Email,
                    DisplayName = $"{user.FirstName} {user.LastName}",
                    MobilePhone = user.ContactNumber,
                    PasswordProfile = new PasswordProfile
                    {
                        ForceChangePasswordNextSignIn = false,
                        Password = user.Password,
                    },
                    PasswordPolicies = "DisablePasswordExpiration",
                    AccountEnabled = true,
                    MailNickname = user.Email.Split('@')[0],
                    Identities = new List<ObjectIdentity>
                    {
                        new ObjectIdentity()
                        {
                            SignInType = "emailAddress",
                            Issuer = _clientOptions.Domain,
                            IssuerAssignedId = user.Email
                        }
                    },
                };
            }
            else
            {
                graphUser = new User()
                {
                    GivenName = user.FirstName,
                    Surname = user.LastName,
                    Mail = user.Email,
                    DisplayName = $"{user.FirstName} {user.LastName}",
                    MobilePhone = user.ContactNumber,
                    PasswordProfile = new PasswordProfile
                    {
                        ForceChangePasswordNextSignIn = false,
                        Password = user.Password,
                    },
                    PasswordPolicies = "DisablePasswordExpiration",
                    AccountEnabled = true,
                    MailNickname = user.Email.Split('@')[0],
                    Identities = new List<ObjectIdentity>
                    {
                        new ObjectIdentity()
                        {
                            SignInType = "emailAddress",
                            Issuer = _adminOptions.Domain,
                            IssuerAssignedId = user.Email
                        }
                    },
                };
            }
    
            return await client.Users.PostAsync(graphUser);    
        }
        public async Task<B2CTokenResponse> InitiateLogin(UserLoginDto loginDto, UserType userType = UserType.Client)
        {
            var userCheck = await GetUserBySignInName(loginDto.Email, userType);
            if (userCheck == null)
            {
                throw new UDValiationException("NO_ACCOUNT_WITH_EMAIL");
            }

            if (userCheck.AccountEnabled == false) 
            { 
                throw new UDValiationException("ACCOUNT_LOCKED_DEFAULT"); 
            }

            string payload;

            if (userType.Equals(UserType.Client))
            {
                payload = $"username={loginDto.Email}&password={loginDto.Password}&grant_type=password&scope=openid+{_clientOptions.ClientId}+offline_access&client_id={_clientOptions.ClientId}&response_type=token+id_token";
            }
            else
            {
                payload = $"username={loginDto.Email}&password={loginDto.Password}&grant_type=password&scope=openid+{_adminOptions.ClientId}+offline_access&client_id={_adminOptions.ClientId}&response_type=token+id_token";
            }

            string _b2cFlowUri = "https://codiceb2ctest.b2clogin.com/codiceb2ctest.onmicrosoft.com/B2C_1_SignIn_ROPC/oauth2/v2.0/token";

            var content = new StringContent(payload);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            HttpResponseMessage response;
            
            response = await _httpClient.PostAsync(_b2cFlowUri, content);
          
            if (!response.IsSuccessStatusCode)
            {
                throw new UDValiationException("EMAIL_PASSWORD_INCORRECT");
            }
            response.EnsureSuccessStatusCode();
       

            string responseString = await response.Content.ReadAsStringAsync();

            B2CTokenResponse tokenResponse = JsonConvert.DeserializeObject<B2CTokenResponse>(responseString);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
           
                throw new Exception("ACCESS_TOKEN_FAILED");
            }
            tokenResponse.Email = loginDto.Email;
            return tokenResponse;
        }
        public void DeleteUser(User user)
        {
            throw new NotImplementedException();
        }

        public User GetByLoginEmail(string email)
        {
            throw new NotImplementedException();
        }

        public User UpdateExistingUser(UserDto user)
        {
            throw new NotImplementedException();
        }

        public async Task ResetUserPassword(string email, string newPassword, UserType userType = UserType.Client)
        {
            var client = GetServiceClient(userType);
            var user = await GetUserBySignInName(email, userType);
            if (user == null)
            {
                throw new UDValiationException("NO_ACCOUNT_WITH_EMAIL");
            }

            user.PasswordProfile = new PasswordProfile
            {
                ForceChangePasswordNextSignIn = false,
                Password = newPassword,
            };

            await client.Users[user.Id].PatchAsync(user);
        }

        public async Task DeactivateUser(string email, UserType userType = UserType.Client)
        {
            GraphServiceClient client = GetServiceClient(userType);
            User user = await GetUserBySignInName(email, userType);
            if (user == null)
            {
                throw new UDValiationException("NO_ACCOUNT_WITH_EMAIL");
            }
            user.AccountEnabled = false;
            await client.Users[user.Id].PatchAsync(user);
        }


        public async Task<User> GetUserBySignInName(string email, UserType userType = UserType.Client)
        {
            var client = GetServiceClient(userType);
            
            string filterQuery;

            if (userType == UserType.Client)
            {
                filterQuery = $"identities/any(c:c/issuerAssignedId eq '{email}' and c/issuer eq '{_clientOptions.TenantId}')";
            }
            else
            {
                filterQuery = $"identities/any(c:c/issuerAssignedId eq '{email}' and c/issuer eq '{_adminOptions.TenantId}')";
            }
            var result1 = await client.Users.GetAsync(requestConfig => requestConfig.QueryParameters.Filter = filterQuery);
            return result1!.Value.FirstOrDefault();

        }

        
    }
}

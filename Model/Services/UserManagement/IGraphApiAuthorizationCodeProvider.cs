

using Azure.Core;
using Core.Enums;
using Microsoft.Graph;

namespace Model.Services.UserManagement
{
    public interface IGraphApiAuthorizationCodeProvider
    {
        TokenCredential GetAuthProvider(UserType userType);
        Task<string> GetAccessTokenAsync(UserType userType);
        GraphServiceClient GetGraphClientWithManagedIdentityOrDevClient(UserType userType);
    }
}

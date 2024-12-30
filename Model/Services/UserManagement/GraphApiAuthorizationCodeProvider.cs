using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Model.OptionModels;
using System;
using Core.Enums;

namespace Model.Services.UserManagement
{
    public class GraphApiAuthorizationCodeProvider : IGraphApiAuthorizationCodeProvider
    {
        private readonly GraphApiCredentialOptionsForClient _clientOptions;
        private readonly GraphApiCredentialOptionsForAdmin _adminOptions;
        private GraphServiceClient? _graphServiceClient;
        public GraphApiAuthorizationCodeProvider(IOptions<GraphApiCredentialOptionsForClient> clientTenentOptions, IOptions<GraphApiCredentialOptionsForAdmin> adminTenentOptions)
        {
            _clientOptions = clientTenentOptions.Value;
            _adminOptions = adminTenentOptions.Value;
        }

        public TokenCredential GetAuthProvider(UserType userType = UserType.Client)
        {
            if (userType.Equals(UserType.Client))
            {
                return new ClientSecretCredential(
               _clientOptions.TenantId,
               _clientOptions.ClientId,
               _clientOptions.GraphApiClientSecret
               );
            }
            else
            {
                return new ClientSecretCredential(
               _adminOptions.TenantId,
               _adminOptions.ClientId,
               _adminOptions.GraphApiClientSecret
               );
            }
            
        }

        public async Task<string> GetAccessTokenAsync(UserType userType)
        {
            if (userType.Equals(UserType.Client)) 
            {
                var authority = $"https://login.microsoftonline.com/{_clientOptions.TenantId}/v2.0";

                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_clientOptions.ClientId)
                    .WithClientSecret(_clientOptions.GraphApiClientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();

                var result = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();

                return result.AccessToken;
            }
            else
            {
                var authority = $"https://login.microsoftonline.com/{_adminOptions.TenantId}/v2.0";

                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_adminOptions.ClientId)
                    .WithClientSecret(_adminOptions.GraphApiClientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();

                var result = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();

                return result.AccessToken;
            }
        }

        public GraphServiceClient GetGraphClientWithManagedIdentityOrDevClient(UserType userType)
        {
            if (_graphServiceClient != null)
                return _graphServiceClient;

            string[] scopes = new[] { "https://graph.microsoft.com/.default" };

            var chainedTokenCredential = GetChainedTokenCredentials(userType);
            _graphServiceClient = new GraphServiceClient(chainedTokenCredential, scopes);

            return _graphServiceClient;
        }

        private ChainedTokenCredential GetChainedTokenCredentials(UserType userType)
        {

            if (userType.Equals(UserType.Client))
            {
                var tenantId = _clientOptions.TenantId;
                var clientId = _clientOptions.ClientId;
                var clientSecret = _clientOptions.GraphApiClientSecret;

                var options = new TokenCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };

                // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
                var devClientSecretCredential = new ClientSecretCredential(
                    tenantId, clientId, clientSecret, options);

                var chainedTokenCredential = new ChainedTokenCredential(devClientSecretCredential);

                return chainedTokenCredential;
            }
            else
            {
                var tenantId = _adminOptions.TenantId;
                var clientId = _adminOptions.ClientId;
                var clientSecret = _adminOptions.GraphApiClientSecret;

                var options = new TokenCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };

                // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
                var devClientSecretCredential = new ClientSecretCredential(
                    tenantId, clientId, clientSecret, options);

                var chainedTokenCredential = new ChainedTokenCredential(devClientSecretCredential);

                return chainedTokenCredential;
            }
        }
    }
}

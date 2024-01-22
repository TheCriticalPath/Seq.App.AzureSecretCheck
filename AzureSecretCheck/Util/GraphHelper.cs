//https://learn.microsoft.com/en-us/graph/tutorials/dotnet?tabs=aad&tutorial-step=2
//https://learn.microsoft.com/en-us/graph/api/resources/application?view=graph-rest-1.0

using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Me.SendMail;

namespace Seq.App.AzureSecretCheck
{
    public class GraphHelper
    {
        // Settings object
        private static Settings? _settings;
        // User auth token credential
        // Client configured with user authentication
        private static GraphServiceClient? _userClient;

        public static void InitializeGraphForUserAuth(Settings settings)
        {
            _settings = settings;

            // The client credentials flow requires that you request the
            // /.default scope, and pre-configure your permissions on the
            // app registration in Azure. An administrator must grant consent
            // to those permissions beforehand.
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            // Values from app registration
            var clientId = settings.ClientId;
            var tenantId = settings.TenantId;
            var clientSecret = settings.ClientSecret;

            // using Azure.Identity;
            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            // https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            _userClient = new GraphServiceClient(clientSecretCredential, settings.GraphScopes);
            //_userClient = new GraphServiceClient(_deviceCodeCredential, settings.GraphUserScopes);
        }
        public static async Task<AzureApplication> GetAzureApplication(string appId){
            AzureApplication application = new AzureApplication(await _userClient.Applications[$"{appId}"].GetAsync());
            return (application);
        }


       
    }
}


using System;
using System.Threading.Tasks;
using System.Web.WebPages;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Orchard.Environment.Extensions;

namespace Orchard.OpenId.Services.AzureActiveDirectory {
    [OrchardFeature("Orchard.OpenId.AzureActiveDirectory")]
    public class AzureActiveDirectoryService : IAzureActiveDirectoryService {
        public string Token { get; set; }
        public DateTimeOffset TokenExpiresOn { get; set; }
        public string AzureTenant { get; set; }

        public async Task<string> AcquireTokenAsync() {
            if (Token == null || Token.IsEmpty())
            {
                throw new Exception("Authorization Required.");
            }
            return await Task.FromResult(Token);
        }

        public ActiveDirectoryClient GetActiveDirectoryClient() {
            var baseServiceUri = new Uri("https://graph.windows.net/");

            var activeDirectoryClient = new ActiveDirectoryClient(new Uri(baseServiceUri, AzureTenant),
                async () => await AcquireTokenAsync());

            return activeDirectoryClient;
        }
    }
}
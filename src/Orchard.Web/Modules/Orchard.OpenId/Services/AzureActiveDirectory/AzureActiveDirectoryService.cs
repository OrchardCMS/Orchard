using System;
using System.Threading.Tasks;
using System.Web.WebPages;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Orchard.Environment.Extensions;

namespace Orchard.OpenId.Services {
    [OrchardFeature("Orchard.OpenId.AzureActiveDirectory")]
    public class AzureActiveDirectoryService {
        public static string Token;
        public static DateTimeOffset TokenExpiresOn;
        public static string AzureTenant = string.Empty;

        public static async Task<string> AcquireTokenAsync() {
            if (Token == null || Token.IsEmpty()) {
                throw new Exception("Authorization Required.");
            }
            return Token;
        }

        public static ActiveDirectoryClient GetActiveDirectoryClient() {
            var baseServiceUri = new Uri("https://graph.windows.net/");

            var activeDirectoryClient = new ActiveDirectoryClient(new Uri(baseServiceUri, AzureTenant),
                async () => await AcquireTokenAsync());

            return activeDirectoryClient;
        }
    }
}
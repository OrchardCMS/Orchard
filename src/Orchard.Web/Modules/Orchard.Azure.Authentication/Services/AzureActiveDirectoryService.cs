using System;
using System.Threading.Tasks;
using System.Web.WebPages;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Orchard.Azure.Authentication.Constants;

namespace Orchard.Azure.Authentication.Services {
    public class AzureActiveDirectoryService {
        public static string token;
        public static DateTimeOffset tokenExpiresOn;
        public static string azureGraphApiUri = DefaultAzureSettings.GraphiApiUri;
        public static string azureTenant = DefaultAzureSettings.Tenant;

        public static async Task<string> AcquireTokenAsync() {
            if (token == null || token.IsEmpty()) {
                throw new Exception("Authorization Required.");
            }
            return token;
        }

        public static ActiveDirectoryClient GetActiveDirectoryClient() {
            var baseServiceUri = new Uri("https://graph.windows.net/");

            var activeDirectoryClient = new ActiveDirectoryClient(new Uri(baseServiceUri, azureTenant),
                async () => await AcquireTokenAsync());

            return activeDirectoryClient;
        }
    }
}
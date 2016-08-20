using System.Linq;
using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace Orchard.Azure.Authentication.Services {
    public class AzureGraphiApiService : IAzureGraphiApiService {
        public IUser GetAzureUser(string userName) {
            var client = AzureActiveDirectoryService.GetActiveDirectoryClient();
            var azureUser = client.Users.Where(x => x.UserPrincipalName == userName).ExecuteAsync().Result.CurrentPage.FirstOrDefault();
            return azureUser;
        }
    }
}
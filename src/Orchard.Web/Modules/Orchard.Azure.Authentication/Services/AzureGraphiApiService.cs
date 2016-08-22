using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace Orchard.Azure.Authentication.Services {
    public class AzureGraphiApiService : IAzureGraphiApiService {
        private readonly ActiveDirectoryClient _client;

        public AzureGraphiApiService() {
            _client = AzureActiveDirectoryService.GetActiveDirectoryClient();
        }

        public IUser GetUser(string userName) {
            var azureUser = _client.Users.Where(x => x.UserPrincipalName == userName)
                .ExecuteAsync().Result.CurrentPage.FirstOrDefault();
            return azureUser;
        }

        public IList<Group> GetUserGroups(string userName) {
            var user = GetUser(userName);
            var userFetcher = (IUserFetcher) user;
            var groups = userFetcher.MemberOf.ExecuteAsync()
                .Result.CurrentPage.Select(x => x as Group).ToList();
            groups.RemoveAll(x => x == null);
            return groups;
        }
    }
}
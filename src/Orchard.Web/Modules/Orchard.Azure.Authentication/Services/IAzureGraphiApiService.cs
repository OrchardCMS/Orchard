using System.Collections.Generic;
using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace Orchard.Azure.Authentication.Services {
    public interface IAzureGraphiApiService : IDependency {
        IUser GetUser(string userName);
        IList<Group> GetUserGroups(string userObjectId);
    }
}
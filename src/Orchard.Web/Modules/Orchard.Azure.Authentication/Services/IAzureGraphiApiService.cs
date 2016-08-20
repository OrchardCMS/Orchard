using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace Orchard.Azure.Authentication.Services {
    public interface IAzureGraphiApiService : IDependency {
        IUser GetAzureUser(string userName);
    }
}
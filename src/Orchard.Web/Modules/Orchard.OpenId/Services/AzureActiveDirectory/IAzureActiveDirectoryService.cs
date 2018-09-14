using System;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace Orchard.OpenId.Services.AzureActiveDirectory {
    public interface IAzureActiveDirectoryService : ISingletonDependency {
        string Token { get; set; }
        DateTimeOffset TokenExpiresOn { get; set; }
        string AzureTenant { get; set; }
        Task<string> AcquireTokenAsync();
        ActiveDirectoryClient GetActiveDirectoryClient();
    }
}
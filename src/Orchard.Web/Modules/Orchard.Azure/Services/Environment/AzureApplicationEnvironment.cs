using Microsoft.WindowsAzure.ServiceRuntime;
using Orchard.Environment;

namespace Orchard.Azure.Services.Environment {
    public class AzureApplicationEnvironment : IApplicationEnvironment {
        public string GetEnvironmentIdentifier() {
            return RoleEnvironment.CurrentRoleInstance.Id;
        }
    }
}
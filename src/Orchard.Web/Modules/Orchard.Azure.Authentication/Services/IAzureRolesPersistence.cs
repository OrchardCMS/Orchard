using System.Collections.Generic;
using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace Orchard.Azure.Authentication.Services {
    public interface IAzureRolesPersistence : IDependency {
        void SyncAzureGroupsToOrchardRoles(string userName, IList<Group> azureGroups);
    }
}
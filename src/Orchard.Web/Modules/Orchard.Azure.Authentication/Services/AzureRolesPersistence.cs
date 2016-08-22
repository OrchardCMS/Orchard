using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using NHibernate;
using Orchard.Data;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;

namespace Orchard.Azure.Authentication.Services {
    public class AzureRolesPersistence : IAzureRolesPersistence {
        private readonly IRoleService _roleService;
        private readonly IMembershipService _membershipService;
        private readonly ISession _session;

        public AzureRolesPersistence(IRoleService roleService, IMembershipService membershipService,
            ITransactionManager transactionManager) {
            _roleService = roleService;
            _membershipService = membershipService;
            _session = transactionManager.GetSession();
        }

        public void SyncAzureGroupsToOrchardRoles(string userName, IList<Group> azureGroups) {
            var user = _membershipService.GetUser(userName);

            var matchingOrchardRoles = azureGroups.Select(@group => _roleService.GetRoleByName(@group.DisplayName))
                .Where(orchardRole => orchardRole != null).ToList();

            foreach (var role in matchingOrchardRoles) {
                _session.SaveOrUpdate(new UserRolesPartRecord {
                    Role = role,
                    UserId = user.Id
                });
            }
        }
    }
}
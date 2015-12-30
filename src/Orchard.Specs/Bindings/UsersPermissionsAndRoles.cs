using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Data;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.Specs.Hosting.Orchard.Web;
using TechTalk.SpecFlow;

namespace Orchard.Specs.Bindings {
    [Binding]
    public class UsersPermissionsAndRoles : BindingBase {


        [When(@"I have a role ""(.*)\"" with permissions ""(.*)\""")]
        public void WhenIHaveARoleWithPermissions(string roleName, string permissions) {
            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {
                    var roleService = environment.Resolve<IRoleService>();

                    roleService.CreateRole(roleName);

                    foreach (var permissionName in permissions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)) {
                        roleService.CreatePermissionForRole(roleName, permissionName);
                    }
                }
            });
        }


        [When(@"I have a user ""(.*)\"" with roles ""(.*)\""")]
        public void GivenIHaveCreatedAUser(string username, string roles) {

            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {
                    var memberShipService = environment.Resolve<IMembershipService>();
                    var roleService = environment.Resolve<IRoleService>();
                    var userRoleRepository = environment.Resolve<IRepository<UserRolesPartRecord>>();
                    var user = memberShipService.CreateUser(new CreateUserParams(username, "qwerty123!", username + "@foo.com", "", "", true));

                    foreach (var roleName in roles.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)) {
                        var role = roleService.GetRoleByName(roleName);
                        userRoleRepository.Create(new UserRolesPartRecord { UserId = user.Id, Role = role });
                    }
                }
            });
        }

        [Given(@"I have a user ""(.*)"" with permissions ""(.*)""")]
        public void GivenIHaveAUserWithPermissions(string username, string permissions) {
            var roleName = Guid.NewGuid().ToString("n");
            WhenIHaveARoleWithPermissions(roleName, permissions);
            GivenIHaveCreatedAUser(username, roleName);
        }

        [When(@"I sign in as ""(.*)""")]
        public void WhenISignInAs(string username) {
            var webApp = Binding<WebAppHosting>();
            var logonForm = TableData(
                new { name = "userNameOrEmail", value = username },
                new { name = "password", value = "qwerty123!" });

            webApp.WhenIGoTo("/users/account/logoff");
            webApp.WhenIGoTo("/users/account/logon");
            webApp.WhenIFillIn(logonForm);
            webApp.WhenIHit("Sign In");
            webApp.WhenIAmRedirected();
        }
    }
}

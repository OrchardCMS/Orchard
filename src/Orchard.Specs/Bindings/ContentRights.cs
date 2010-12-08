using System;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents;
using Orchard.Data;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Specs.Hosting.Orchard.Web;
using TechTalk.SpecFlow;

namespace Orchard.Specs.Bindings {
    [Binding]
    public class ContentRights : BindingBase {

        [When(@"I have a role ""(.*)\"" with permissions ""(.*)\""")]
        public void WhenIHaveARoleWithPermissions(string roleName, string permissions) {
            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using ( var environment = MvcApplication.CreateStandaloneEnvironment("Default") ) {
                    var roleService = environment.Resolve<IRoleService>();

                    roleService.CreateRole(roleName);

                    foreach ( var permissionName in permissions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries) ) {
                        roleService.CreatePermissionForRole(roleName, permissionName);
                    }
                }
            });
        }
        
        [When(@"I have a user ""(.*)\"" with roles ""(.*)\""")]
        public void GivenIHaveCreatedAUser(string username, string roles) {

            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using ( var environment = MvcApplication.CreateStandaloneEnvironment("Default") ) {
                    var memberShipService = environment.Resolve<IMembershipService>();
                    var roleService = environment.Resolve<IRoleService>();
                    var userRoleRepository = environment.Resolve<IRepository<UserRolesPartRecord>>();
                    var user = memberShipService.CreateUser(new CreateUserParams(username, "qwerty123!", username + "@foo.com", "", "", true));

                    foreach ( var roleName in roles.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries) ) {
                        var role = roleService.GetRoleByName(roleName);
                        userRoleRepository.Create(new UserRolesPartRecord { UserId = user.Id, Role = role });
                    }
                }
            });
        }

        [Then(@"""(.*)\"" should be able to ""(.*)\"" a ""(.*)\"" owned by ""(.*)\""")]
        public void UserShouldBeAbleToForOthers(string username, string action, string contentType, string otherName) {

            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using ( var environment = MvcApplication.CreateStandaloneEnvironment("Default") ) {
                    var memberShipService = environment.Resolve<IMembershipService>();
                    var athorizationService = environment.Resolve<IAuthorizationService>();
                    var contentManager = environment.Resolve<IContentManager>();

                    var contentItem = contentManager.Create(contentType);
                    var user = memberShipService.GetUser(username);
                    var otherUser = memberShipService.GetUser(otherName);
                    contentItem.As<ICommonPart>().Owner = otherUser;

                    Assert.That(athorizationService.TryCheckAccess(GetPermissionForAction(action), user, contentItem), Is.True);
                }
            });
        }

        [Then(@"""(.*)\"" should not be able to ""(.*)\"" a ""(.*)\"" owned by ""(.*)\""")]
        public void UserShouldNotBeAbleToForOthers(string username, string action, string contentType, string otherName) {

            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using ( var environment = MvcApplication.CreateStandaloneEnvironment("Default") ) {
                    var memberShipService = environment.Resolve<IMembershipService>();
                    var athorizationService = environment.Resolve<IAuthorizationService>();
                    var contentManager = environment.Resolve<IContentManager>();

                    var contentItem = contentManager.Create(contentType);
                    var user = memberShipService.GetUser(username);
                    var otherUser = memberShipService.GetUser(otherName);
                    contentItem.As<ICommonPart>().Owner = otherUser;

                    Assert.That(athorizationService.TryCheckAccess(GetPermissionForAction(action), user, contentItem), Is.False);
                }
            });
        }

        // returns permissions as they are used in controllers for each action
        private static Permission GetPermissionForAction(string action) {
            switch ( action ) {
                case "publish":
                    return Permissions.PublishOthersContent;
                case "edit":
                    return Permissions.EditOthersContent;
                case "delete":
                    return Permissions.DeleteOthersContent;
                default:
                    return null;
            }
        }

    }
}

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

        [Then(@"""(.*)\"" should be able to ""(.*)\"" a ""(.*)\"" owned by ""(.*)\""")]
        public void UserShouldBeAbleToForOthers(string username, string action, string contentType, string otherName) {

            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using ( var environment = MvcApplication.CreateStandaloneEnvironment("Default") ) {
                    var memberShipService = environment.Resolve<IMembershipService>();
                    var authorizationService = environment.Resolve<IAuthorizationService>();
                    var contentManager = environment.Resolve<IContentManager>();

                    var contentItem = contentManager.Create(contentType);
                    var user = memberShipService.GetUser(username);
                    var otherUser = memberShipService.GetUser(otherName);
                    contentItem.As<ICommonPart>().Owner = otherUser;

                    Assert.That(authorizationService.TryCheckAccess(GetPermissionForAction(action), user, contentItem), Is.True);
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
                    return Permissions.PublishContent;
                case "edit":
                    return Permissions.EditContent;
                case "delete":
                    return Permissions.DeleteContent;
                default:
                    return null;
            }
        }

    }
}

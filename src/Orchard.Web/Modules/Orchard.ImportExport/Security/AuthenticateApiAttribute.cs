using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Filters;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Permissions;
using Orchard.ImportExport.Services;
using Orchard.Logging;
using Orchard.Security;

namespace Orchard.ImportExport.Security {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    [OrchardFeature("Orchard.Deployment")]
    public class AuthenticateApiAttribute : AuthorizationFilterAttribute {
        public ILogger Logger { get; set; }

        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext) {
            var workContext = actionContext.ControllerContext.GetWorkContext();
            var membershipService = workContext.Resolve<IMembershipService>();
            var authenticationService = workContext.Resolve<IAuthenticationService>();
            var authorizationService = workContext.Resolve<IAuthorizationService>();
            var signingService = workContext.Resolve<ISigningService>();

            Logger = NullLogger.Instance;

            try {
                var headers = actionContext.Request.Headers;
                var timeStampString = GetHttpRequestHeader(headers, signingService.TimestampHeaderName);
                var authenticationString = GetHttpRequestHeader(headers, signingService.AuthenticationHeaderName);
                if (string.IsNullOrEmpty(timeStampString) || string.IsNullOrEmpty(authenticationString)) {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    return;
                }

                var authenticationParts = authenticationString.Split(new[] {":"},
                    StringSplitOptions.RemoveEmptyEntries);

                if (authenticationParts.Length != 2) {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    return;
                }

                var username = authenticationParts[0];
                var signature = HttpUtility.UrlDecode(authenticationParts[1]);

                var user = membershipService.GetUser(username);
                var methodType = actionContext.Request.Method.Method;
                var absolutePath = actionContext.Request.RequestUri.AbsolutePath.ToLower();
                var uri = HttpUtility.UrlDecode(absolutePath);
                var deploymentUser = user.ContentItem.As<DeploymentUserPart>();
                var isAuthenticated = signingService.ValidateRequest(methodType, timeStampString, uri, deploymentUser.PrivateApiKey, signature);

                if (isAuthenticated &&
                    (authorizationService.TryCheckAccess(DeploymentPermissions.ImportFromDeploymentSources, user, null) ||
                     authorizationService.TryCheckAccess(DeploymentPermissions.ExportToDeploymentTargets, user, null))) {
                    authenticationService.SetAuthenticatedUserForRequest(user);
                }
                else {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
            }
            catch (Exception ex) {
                Logger.Error("Credentials could not be validated. Ensure they are in the correct format.", ex);
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }

        private static string GetHttpRequestHeader(HttpHeaders headers, string headerName) {
            if (!headers.Contains(headerName)) return string.Empty;

            return headers
                .GetValues(headerName)
                .SingleOrDefault();
        }
    }
}

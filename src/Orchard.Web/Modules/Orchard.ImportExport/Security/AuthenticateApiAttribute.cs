using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.ClientServices;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Security;
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
    public class AuthenticateApiAttribute : ActionFilterAttribute, IAuthenticationFilter {
        public ILogger Logger { get; set; }

        public void OnAuthentication(AuthenticationContext filterContext) {
            var workContext = filterContext.Controller.ControllerContext.GetWorkContext();
            var membershipService = workContext.Resolve<IMembershipService>();
            var authenticationService = workContext.Resolve<IAuthenticationService>();
            var authorizationService = workContext.Resolve<IAuthorizationService>();
            var signingService = workContext.Resolve<ISigningService>();

            Logger = NullLogger.Instance;

            try {
                var request = filterContext.RequestContext.HttpContext.Request;
                var headers = request.Headers;
                var timeStampString = GetHttpRequestHeader(headers, signingService.TimestampHeaderName);
                var authenticationString = GetHttpRequestHeader(headers, signingService.AuthenticationHeaderName);
                if (string.IsNullOrEmpty(timeStampString) || string.IsNullOrEmpty(authenticationString)) {
                    filterContext.Result = new HttpUnauthorizedResult();
                    return;
                }

                var authenticationParts = authenticationString.Split(new[] {":"},
                    StringSplitOptions.RemoveEmptyEntries);

                if (authenticationParts.Length != 2) {
                    filterContext.Result = new HttpUnauthorizedResult();
                    return;
                }

                var username = authenticationParts[0];
                var signature = HttpUtility.UrlDecode(authenticationParts[1]);

                var user = membershipService.GetUser(username);
                var methodType = request.HttpMethod;
                var absolutePath = request.Url.AbsolutePath.ToLower();
                var uri = HttpUtility.UrlDecode(absolutePath);
                var deploymentUser = user.ContentItem.As<DeploymentUserPart>();
                var isAuthenticated = signingService.ValidateRequest(methodType, timeStampString, uri, deploymentUser.PrivateApiKey, signature);
                if (isAuthenticated &&
                    (authorizationService.TryCheckAccess(DeploymentPermissions.ImportFromDeploymentSources, user, null) ||
                     authorizationService.TryCheckAccess(DeploymentPermissions.ExportToDeploymentTargets, user, null))) {
                    filterContext.Principal = new ApiPrincipal(user);
                    authenticationService.SetAuthenticatedUserForRequest(user);
                }
                else {
                    filterContext.Result = new HttpUnauthorizedResult();
                }
            }
            catch (Exception ex) {
                Logger.Error("Credentials could not be validated. Ensure they are in the correct format.", ex);
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext) {
            var user = filterContext.HttpContext.User;
            if (user == null || !user.Identity.IsAuthenticated) {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        private static string GetHttpRequestHeader(NameValueCollection headers, string headerName) {
            return headers[headerName] ?? "";
        }
    }
}

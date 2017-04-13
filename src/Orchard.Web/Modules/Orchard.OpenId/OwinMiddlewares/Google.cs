using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Microsoft.Owin.Security.Google;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.OpenId.Models;
using Orchard.Owin;
using Owin;

namespace Orchard.OpenId.OwinMiddlewares {
    [OrchardFeature("Orchard.OpenId.Google")]
    public class Google : IOwinMiddlewareProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public Google(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            var workContext = _workContextAccessor.GetContext();
            var settings = workContext.CurrentSite.As<GoogleSettingsPart>();

            if (settings == null || !settings.IsValid()) {
                return Enumerable.Empty<OwinMiddlewareRegistration>();
            }

            var authenticationOptions = new GoogleOAuth2AuthenticationOptions {
                ClientId = settings.ClientId,
                ClientSecret = settings.ClientSecret,
                CallbackPath = new PathString(GetCallbackPath(workContext, settings))
            };

            return new List<OwinMiddlewareRegistration> {
                new OwinMiddlewareRegistration {
                    Priority = Constants.General.OpenIdOwinMiddlewarePriority,
                    Configure = app => {
                        app.UseGoogleAuthentication(authenticationOptions);
                    }
                }
            };
        }

        private string GetCallbackPath(WorkContext workContext, GoogleSettingsPart settings) {
            var shellSettings = workContext.Resolve<ShellSettings>();
            var tenantPrefix = shellSettings.RequestUrlPrefix;

            var callbackUrl = string.IsNullOrWhiteSpace(tenantPrefix) ?
                settings.CallbackPath :
                string.Format("/{0}{1}", tenantPrefix, settings.CallbackPath);

            return callbackUrl;
        }
    }
}
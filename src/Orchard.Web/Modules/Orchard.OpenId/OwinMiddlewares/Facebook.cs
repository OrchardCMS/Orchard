using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.OpenId.Models;
using Orchard.Owin;
using Owin;

namespace Orchard.OpenId.OwinMiddlewares {
    [OrchardFeature("Orchard.OpenId.Facebook")]
    public class Facebook : IOwinMiddlewareProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public Facebook(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            var settings = _workContextAccessor.GetContext().CurrentSite.As<FacebookSettingsPart>();

            if (settings == null || !settings.IsValid()) {
                return Enumerable.Empty<OwinMiddlewareRegistration>();
            }

            return new List<OwinMiddlewareRegistration> {
                new OwinMiddlewareRegistration {
                    Priority = Constants.General.OpenIdOwinMiddlewarePriority,
                    Configure = app => {
                        app.UseFacebookAuthentication(
                             appId: settings.AppId,
                             appSecret: settings.AppSecret
                        );
                    }
                }
            };
        }
    }
}
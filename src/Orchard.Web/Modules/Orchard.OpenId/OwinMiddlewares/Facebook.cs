using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Owin;
using Owin;

namespace Orchard.OpenId.OwinMiddlewares {
    [OrchardFeature("Orchard.OpenId.Facebook")]
    public class Facebook : IOwinMiddlewareProvider {
        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            // TODO: change these into site settings
            var appId = "";
            var appSecret = "";

            return new List<OwinMiddlewareRegistration> {
                new OwinMiddlewareRegistration {
                    Priority = Constants.OpenIdOwinMiddlewarePriority,
                    Configure = app => {
                        app.UseFacebookAuthentication(
                             appId: appId,
                             appSecret: appSecret
                        );
                    }
                }
            };
        }
    }
}
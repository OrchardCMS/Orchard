using System.Collections.Generic;
using Microsoft.Owin;
using Microsoft.Owin.Security.Google;
using Orchard.Environment.Extensions;
using Orchard.Owin;
using Owin;

namespace Orchard.OpenId.OwinMiddlewares {
    [OrchardFeature("Orchard.OpenId.Google")]
    public class Google : IOwinMiddlewareProvider {
        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            // TODO: change these into site settings
            // Placeholders needed because Google's OwinMiddleware will throws an error if empty
            var clientId = "000-000.apps.googleusercontent.com";
            var clientSecret = "a-aaaaaaaaaaaaaaaaaaaaaa";
            var callbackPath = new PathString(Constants.LogonCallbackUrl);

            var authenticationOptions = new GoogleOAuth2AuthenticationOptions {
                ClientId = clientId,
                ClientSecret = clientSecret,
                CallbackPath = callbackPath
            };

            return new List<OwinMiddlewareRegistration> {
                new OwinMiddlewareRegistration {
                    Priority = Constants.OpenIdOwinMiddlewarePriority,
                    Configure = app => {
                        app.UseGoogleAuthentication(authenticationOptions);
                    }
                }
            };
        }
    }
}
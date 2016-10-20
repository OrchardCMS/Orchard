using System.Collections.Generic;
using Microsoft.Owin.Security.OpenIdConnect;
using Orchard.Environment.Extensions;
using Orchard.Owin;
using Owin;

namespace Orchard.OpenId.OwinMiddlewares {
    [OrchardFeature("Orchard.OpenId.ActiveDirectoryFederationServices")]
    public class ActiveDirectoryFederationServices : IOwinMiddlewareProvider {
        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            // TODO: change these into site settings
            string clientId = "";
            string metadataAddress = "";
            string postLogoutRedirectUri = "";

            var openIdOptions = new OpenIdConnectAuthenticationOptions {
                ClientId = clientId,
                MetadataAddress = metadataAddress,
                RedirectUri = postLogoutRedirectUri,
                PostLogoutRedirectUri = postLogoutRedirectUri
            };

            return new List<OwinMiddlewareRegistration> {
                new OwinMiddlewareRegistration {
                    Priority = Constants.OpenIdOwinMiddlewarePriority,
                    Configure = app => {
                        app.UseOpenIdConnectAuthentication(openIdOptions);
                    }
                }
            };
        }
    }
}
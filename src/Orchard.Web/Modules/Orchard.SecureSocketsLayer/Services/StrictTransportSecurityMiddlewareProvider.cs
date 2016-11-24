using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Owin;
using Orchard.SecureSocketsLayer.Models;
using Owin;

namespace Orchard.SecureSocketsLayer.Services {
    public class StrictTransportSecurityMiddlewareProvider : IOwinMiddlewareProvider {
        private readonly IWorkContextAccessor _wca;

        public ILogger Logger { get; set; }

        public StrictTransportSecurityMiddlewareProvider(IWorkContextAccessor wca) {
            _wca = wca;

            Logger = NullLogger.Instance;
        }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            return new[] {
                new OwinMiddlewareRegistration {
                    Configure = app =>
                        app.Use(async (context, next) => {
                            var sslSettings = _wca.GetContext().CurrentSite.As<SslSettingsPart>();

                            if (sslSettings.SendStrictTransportSecurityHeaders) {
                                string responseValue = "max-age=" + sslSettings.StrictTransportSecurityMaxAge;

                                if (sslSettings.StrictTransportSecurityIncludeSubdomains) {
                                    responseValue += "; includeSubDomains";
                                }

                                if (sslSettings.StrictTransportSecurityPreload) {
                                    responseValue += "; preload";
                                }
                                context.Response.Headers.Append("Strict-Transport-Security", responseValue);
                            }

                            await next.Invoke();
                        })
                }
            };
        }
    }
}
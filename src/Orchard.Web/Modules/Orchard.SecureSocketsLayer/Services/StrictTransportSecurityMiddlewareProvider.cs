using System.Collections.Generic;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Owin;
using Orchard.SecureSocketsLayer.Models;
using Owin;

namespace Orchard.SecureSocketsLayer.Services {
    public class StrictTransportSecurityMiddlewareProvider : IOwinMiddlewareProvider {
        private readonly IWorkContextAccessor _wca;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public ILogger Logger { get; set; }

        public StrictTransportSecurityMiddlewareProvider(
            IWorkContextAccessor wca,
            ICacheManager cacheManager,
            ISignals signals) {

            _wca = wca;
            _cacheManager = cacheManager;
            _signals = signals;

            Logger = NullLogger.Instance;
        }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            return new[] {
                new OwinMiddlewareRegistration {
                    Configure = app =>
                        app.Use(async (context, next) => {
                            var cacheKey = "Orchard.SecureSocketsLayer.Services.StrictTransportSecurityMiddlewareProvider.GetOwinMiddlewares";
                            var sslSettings = _cacheManager.Get(cacheKey, true, ctx =>{
                                // check whether the cache should be invalidated
                                ctx.Monitor(_signals.When("SslSettingsPart_EvictAll"));
                                // cache this and save recomputing it each call
                                return _wca.GetContext().CurrentSite.As<SslSettingsPart>();
                            });

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
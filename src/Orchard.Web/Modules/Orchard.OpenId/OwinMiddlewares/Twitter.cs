using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Twitter;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.OpenId.Models;
using Orchard.Owin;
using Owin;

namespace Orchard.OpenId.OwinMiddlewares {
    [OrchardFeature("Orchard.OpenId.Twitter")]
    public class Twitter : IOwinMiddlewareProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public Twitter(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            var settings = _workContextAccessor.GetContext().CurrentSite.As<TwitterSettingsPart>();

            if (settings == null || !settings.IsValid()) {
                return Enumerable.Empty<OwinMiddlewareRegistration>();
            }

            var twitterOptions = new TwitterAuthenticationOptions {
                ConsumerKey = settings.ConsumerKey,
                ConsumerSecret = settings.ConsumerSecret,
                BackchannelCertificateValidator = new CertificateSubjectKeyIdentifierValidator(new[]
                {
                    settings.VeriSignClass3SecureServerCA_G2,
                    settings.VeriSignClass3SecureServerCA_G3,
                    settings.VeriSignClass3PublicPrimaryCA_G5,
                    settings.SymantecClass3SecureServerCA_G4,
                    settings.DigiCertSHA2HighAssuranceServerCA,
                    settings.DigiCertHighAssuranceEVRootCA 
                })
            };

            return new List<OwinMiddlewareRegistration> {
                new OwinMiddlewareRegistration {
                    Priority = Constants.General.OpenIdOwinMiddlewarePriority,
                    Configure = app => {
                        app.UseTwitterAuthentication(twitterOptions);
                    }
                }
            };
        }
    }
}
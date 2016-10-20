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

            if (settings == null || !settings.IsValid) {
                return Enumerable.Empty<OwinMiddlewareRegistration>();
            }

            var twitterOptions = new TwitterAuthenticationOptions {
                ConsumerKey = settings.ConsumerKey,
                ConsumerSecret = settings.ConsumerSecret,
                BackchannelCertificateValidator = new CertificateSubjectKeyIdentifierValidator(new[]
                {
                    Constants.VeriSignClass3SecureServerCA_G2,
                    Constants.VeriSignClass3SecureServerCA_G3,
                    Constants.VeriSignClass3PublicPrimaryCA_G5,
                    Constants.SymantecClass3SecureServerCA_G4,
                    Constants.DigiCertSHA2HighAssuranceServerCA,
                    Constants.DigiCertHighAssuranceEVRootCA 
                })
            };

            return new List<OwinMiddlewareRegistration> {
                new OwinMiddlewareRegistration {
                    Priority = Constants.OpenIdOwinMiddlewarePriority,
                    Configure = app => {
                        app.UseTwitterAuthentication(twitterOptions);
                    }
                }
            };
        }
    }
}
using System.Collections.Generic;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Twitter;
using Orchard.Environment.Extensions;
using Orchard.Owin;
using Owin;

namespace Orchard.OpenId.OwinMiddlewares {
    [OrchardFeature("Orchard.OpenId.Twitter")]
    public class Twitter : IOwinMiddlewareProvider {

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            // TODO: change these into site settings
            var consumerKey = "";
            var consumerSecret = "";

            var twitterOptions = new TwitterAuthenticationOptions {
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
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
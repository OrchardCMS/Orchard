﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.Security.OpenIdConnect;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.OpenId.Models;
using Orchard.Owin;
using Owin;

namespace Orchard.OpenId.OwinMiddlewares {
    [OrchardFeature("Orchard.OpenId.ActiveDirectoryFederationServices")]
    public class ActiveDirectoryFederationServices : IOwinMiddlewareProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public ActiveDirectoryFederationServices(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            var settings = _workContextAccessor.GetContext().CurrentSite.As<ActiveDirectoryFederationServicesSettingsPart>();

            if (settings == null || !settings.IsValid) {
                return Enumerable.Empty<OwinMiddlewareRegistration>();
            }

            var openIdOptions = new OpenIdConnectAuthenticationOptions {
                ClientId = settings.ClientId,
                MetadataAddress = settings.MetadataAddress,
                RedirectUri = settings.PostLogoutRedirectUri,
                PostLogoutRedirectUri = settings.PostLogoutRedirectUri
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
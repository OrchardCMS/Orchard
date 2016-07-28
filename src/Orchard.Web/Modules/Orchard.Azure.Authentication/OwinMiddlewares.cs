using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Web.Helpers;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OpenIdConnect;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Owin;
using Orchard.Settings;
using Owin;
using Orchard.Azure.Authentication.Models;
using Orchard.Azure.Authentication.Security;

namespace Orchard.Azure.Authentication {
    public class OwinMiddlewares : IOwinMiddlewareProvider {
        public ILogger Logger { get; set; }
       
        private readonly string _azureClientId;
        private readonly string _azureTenant;
        private readonly string _azureADInstance;
        private readonly string _logoutRedirectUri;
        private readonly string _azureAppName;
        private readonly bool _sslEnabled;
        private readonly bool _azureWebSiteProtectionEnabled;

        public OwinMiddlewares(ISiteService siteService) {
            Logger = NullLogger.Instance;

            var site = siteService.GetSiteSettings();
            var azureSettings = site.As<AzureSettingsPart>();

            _azureClientId = ((azureSettings.ClientId == null) || (azureSettings.ClientId == string.Empty)) ? 
                "[example: 82692da5-a86f-44c9-9d53-2f88d52b478b]" : azureSettings.ClientId;

            _azureTenant = ((azureSettings.Tenant == null) || (azureSettings.Tenant == string.Empty)) ? 
                "faketenant.com" : azureSettings.Tenant;

            _azureADInstance = ((azureSettings.ADInstance == null) || (azureSettings.ADInstance == string.Empty)) ? 
                "https://login.microsoft.com/{0}" : azureSettings.ADInstance;

            _logoutRedirectUri = ((azureSettings.LogoutRedirectUri == null) || (azureSettings.LogoutRedirectUri == string.Empty)) ? 
                site.BaseUrl : azureSettings.LogoutRedirectUri;

            _azureAppName = ((azureSettings.AppName == null) || (azureSettings.AppName == string.Empty)) ? 
                "[example: MyAppName]" : azureSettings.AppName;

            _sslEnabled = azureSettings.SSLEnabled;

            _azureWebSiteProtectionEnabled = azureSettings.AzureWebSiteProtectionEnabled;
        }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            var middlewares = new List<OwinMiddlewareRegistration>();

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            var openIdOptions = new OpenIdConnectAuthenticationOptions {
                ClientId = _azureClientId,
                Authority = string.Format(CultureInfo.InvariantCulture, _azureADInstance, _azureTenant),
                PostLogoutRedirectUri = _logoutRedirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications ()
            };

            var cookieOptions = new CookieAuthenticationOptions();

            if (_azureWebSiteProtectionEnabled) {
                middlewares.Add(new OwinMiddlewareRegistration {
                    Priority = "9",
                    Configure = app => { app.SetDataProtectionProvider(new MachineKeyProtectionProvider()); }
                });
            }

            middlewares.Add(new OwinMiddlewareRegistration {
                Priority = "10",
                Configure = app => {
                    app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

                    app.UseCookieAuthentication(cookieOptions);

                    app.UseOpenIdConnectAuthentication(openIdOptions);
                }
            });

            return middlewares;
        }
    }
}
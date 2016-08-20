using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
using Orchard.Azure.Authentication.Constants;
using Orchard.Azure.Authentication.Models;
using Orchard.Azure.Authentication.Security;

namespace Orchard.Azure.Authentication
{
    public class OwinMiddlewares : IOwinMiddlewareProvider
    {
        public ILogger Logger { get; set; }

        private readonly string _azureClientId = DefaultAzureSettings.ClientId;
        private readonly string _azureTenant = DefaultAzureSettings.Tenant;
        private readonly string _logoutRedirectUri = DefaultAzureSettings.LogoutRedirectUri;
        private readonly string _azureAdInstance = DefaultAzureSettings.ADInstance;
        private readonly bool _azureWebSiteProtectionEnabled = DefaultAzureSettings.AzureWebSiteProtectionEnabled;
        private readonly string _relativePostLoginCallBackUrl = "Orchard.Azure.Authentication/Account/LogOnCallBack";


        public OwinMiddlewares(ISiteService siteService)
        {
            Logger = NullLogger.Instance;

            try
            {
                var settings = siteService.GetSiteSettings().As<AzureSettingsPart>();

                if (settings == null)
                {
                    return;
                }

                _azureClientId = settings.ClientId ?? _azureClientId;
                _azureTenant = settings.Tenant ?? _azureTenant;
                _azureAdInstance = string.IsNullOrEmpty(settings.ADInstance) ?  _azureAdInstance : settings.ADInstance;
                _logoutRedirectUri = settings.LogoutRedirectUri ?? _logoutRedirectUri;
                _azureWebSiteProtectionEnabled = settings.AzureWebSiteProtectionEnabled;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Debug, ex, "An error occured while accessing azure settings: {0}");
            }
        }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares()
        {
            var middlewares = new List<OwinMiddlewareRegistration>();

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            var openIdOptions = new OpenIdConnectAuthenticationOptions
            {
                ClientId = _azureClientId,
                Authority = string.Format(CultureInfo.InvariantCulture, _azureAdInstance, _azureTenant), // e.g. "https://login.windows.net/azurefridays.onmicrosoft.com/"
                PostLogoutRedirectUri = _logoutRedirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = (context) =>
                    {
                        try
                        {
                            //We should be able assign roles based on claims here.  However, some IT departments 
                            //disable some Azure AD functionality for security reasons.  For example, AD Group 
                            //membership data may not be included in these claims for your Azure directory.  
                            //It is safest to read group membership data from the Azure graph API 
                            //(see code in AzureAuthenticationService.cs)

                            //Do consider processing JWT tokens here, to enable calls from this user to related 
                            //Azure Web Services
                            return Task.FromResult(0);
                        }
                        catch (SecurityTokenValidationException ex)
                        {
                            return Task.FromResult(0);
                        }
                    }
                }
            };


            if (false == string.IsNullOrWhiteSpace(_logoutRedirectUri))
            {
                openIdOptions.RedirectUri = _logoutRedirectUri;
                char lastChar = openIdOptions.RedirectUri[openIdOptions.RedirectUri.Length - 1];
                if ('/' != lastChar)
                    openIdOptions.RedirectUri = openIdOptions.RedirectUri + "/";
                openIdOptions.RedirectUri = openIdOptions.RedirectUri + _relativePostLoginCallBackUrl;
            }

            var cookieOptions = new CookieAuthenticationOptions();

            if (_azureWebSiteProtectionEnabled)
            {
                middlewares.Add(new OwinMiddlewareRegistration
                {
                    Priority = "9",
                    Configure = app => { app.SetDataProtectionProvider(new MachineKeyProtectionProvider()); }
                });
            }

            middlewares.Add(new OwinMiddlewareRegistration
            {
                Priority = "10",
                Configure = app =>
                {
                    app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

                    app.UseCookieAuthentication(cookieOptions);

                    app.UseOpenIdConnectAuthentication(openIdOptions);
                }
            });

            return middlewares;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Web.Helpers;
using System.Web.WebPages;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
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
using Orchard.Azure.Authentication.Services;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;
using LogLevel = Orchard.Logging.LogLevel;

namespace Orchard.Azure.Authentication {
    public class OwinMiddlewares : IOwinMiddlewareProvider {
        public ILogger Logger { get; set; }

        private readonly string _azureClientId = DefaultAzureSettings.ClientId;
        private readonly string _azureTenant = DefaultAzureSettings.Tenant;
        private readonly string _logoutRedirectUri = DefaultAzureSettings.LogoutRedirectUri;
        private readonly string _azureAdInstance = DefaultAzureSettings.ADInstance;
        private readonly bool _azureWebSiteProtectionEnabled = DefaultAzureSettings.AzureWebSiteProtectionEnabled;
        private readonly string _azureGraphiApiUri = DefaultAzureSettings.GraphiApiUri;
        private readonly string _azureGraphApiKey = DefaultAzureSettings.GraphApiKey;
        private readonly string _relativePostLoginCallBackUrl = "Orchard.Azure.Authentication/Account/LogOnCallBack";


        public OwinMiddlewares(ISiteService siteService) {
            Logger = NullLogger.Instance;

            try {
                var settings = siteService.GetSiteSettings().As<AzureSettingsPart>();

                if (settings == null) {
                    return;
                }

                _azureClientId = settings.ClientId ?? _azureClientId;
                _azureTenant = settings.Tenant ?? _azureTenant;
                _azureAdInstance = string.IsNullOrEmpty(settings.ADInstance) ? _azureAdInstance : settings.ADInstance;
                _logoutRedirectUri = settings.LogoutRedirectUri ?? _logoutRedirectUri;
                _azureWebSiteProtectionEnabled = settings.AzureWebSiteProtectionEnabled;
                _azureGraphiApiUri = settings.GraphApiUrl ?? _azureGraphiApiUri;
            }
            catch (Exception ex) {
                Logger.Log(LogLevel.Debug, ex, "An error occured while accessing azure settings: {0}");
            }
        }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            var middlewares = new List<OwinMiddlewareRegistration>();

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            var openIdOptions = new OpenIdConnectAuthenticationOptions {
                ClientId = _azureClientId,
                Authority = string.Format(CultureInfo.InvariantCulture, _azureAdInstance, _azureTenant), // e.g. "https://login.windows.net/azurefridays.onmicrosoft.com/"
                PostLogoutRedirectUri = _logoutRedirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications()
            };


            if (false == string.IsNullOrWhiteSpace(_logoutRedirectUri)) {
                openIdOptions.RedirectUri = _logoutRedirectUri;
                char lastChar = openIdOptions.RedirectUri[openIdOptions.RedirectUri.Length - 1];
                if ('/' != lastChar)
                    openIdOptions.RedirectUri = openIdOptions.RedirectUri + "/";
                openIdOptions.RedirectUri = openIdOptions.RedirectUri + _relativePostLoginCallBackUrl;
            }

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

            middlewares.Add(new OwinMiddlewareRegistration {
                Priority = "11",
                Configure = app => app.Use(async (context, next) => {
                    try {
                        if (AzureActiveDirectoryService.token == null && AzureActiveDirectoryService.token.IsEmpty()) {
                            RegenerateAzureGraphApiToken();
                        }
                        else {
                            if (DateTimeOffset.Compare(DateTimeOffset.UtcNow, AzureActiveDirectoryService.tokenExpiresOn) > 0) {
                                RegenerateAzureGraphApiToken();
                            }
                        }
                    }
                    catch (Exception ex) {
                        Logger.Log(LogLevel.Error, ex, "An error occured generating azure api credential {0}", ex.Message);
                    }

                    await next.Invoke();
                })
            });

            return middlewares;
        }

        private void RegenerateAzureGraphApiToken() {
            var result = GetAuthContext().AcquireToken("https://graph.windows.net/", GetClientCredential());

            AzureActiveDirectoryService.tokenExpiresOn = result.ExpiresOn;
            AzureActiveDirectoryService.token = result.AccessToken;
            AzureActiveDirectoryService.azureGraphApiUri = "https://graph.windows.net/";
            AzureActiveDirectoryService.azureTenant = _azureTenant;
        }

        private ClientCredential GetClientCredential() {
            return new ClientCredential(_azureClientId, _azureGraphApiKey);
        }

        private AuthenticationContext GetAuthContext() {
            var authority = string.Format(CultureInfo.InvariantCulture, _azureAdInstance, _azureTenant);

            return new AuthenticationContext(authority, false);
        }
    }
}
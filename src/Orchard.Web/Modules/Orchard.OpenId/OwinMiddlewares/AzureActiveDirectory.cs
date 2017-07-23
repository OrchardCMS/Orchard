using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.WebPages;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OpenIdConnect;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.OpenId.Models;
using Orchard.OpenId.Security;
using Orchard.OpenId.Services.AzureActiveDirectory;
using Orchard.Owin;
using Owin;
using LogLevel = Orchard.Logging.LogLevel;

namespace Orchard.OpenId.OwinMiddlewares {
    [OrchardFeature("Orchard.OpenId.AzureActiveDirectory")]
    public class AzureActiveDirectory : IOwinMiddlewareProvider {
        public ILogger Logger { get; set; }

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly InMemoryCache _inMemoryCache;
        private readonly IAzureActiveDirectoryService _azureActiveDirectoryService;
        private string _azureGraphApiUri;
        private string _azureGraphApiKey;
        private string _azureClientId;
        private string _azureTenant;
        private string _azureAdInstance;

        public AzureActiveDirectory(
            IWorkContextAccessor workContextAccessor,
            IAzureActiveDirectoryService azureActiveDirectoryService,
            InMemoryCache inMemoryCache) {
            _workContextAccessor = workContextAccessor;
            _azureActiveDirectoryService = azureActiveDirectoryService;
            _inMemoryCache = inMemoryCache;

            Logger = NullLogger.Instance;
        }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            var settings = _workContextAccessor.GetContext().CurrentSite.As<AzureActiveDirectorySettingsPart>();
            var logoutRedirectUri = string.Empty;
            var azureAppKey = string.Empty;
            var azureWebSiteProtectionEnabled = false;
            var azureUseAzureGraphApi = false;

            if (settings == null || !settings.IsValid()) {
                return Enumerable.Empty<OwinMiddlewareRegistration>();
            }

            _azureClientId = settings.ClientId;
            _azureTenant = settings.Tenant;
            _azureAdInstance = settings.ADInstance;
            _azureGraphApiUri = settings.GraphApiUrl;
            _azureGraphApiKey = settings.GraphApiKey;
            logoutRedirectUri = settings.LogoutRedirectUri;
            azureWebSiteProtectionEnabled = settings.AzureWebSiteProtectionEnabled;
            azureAppKey = settings.AppKey;
            azureUseAzureGraphApi = settings.UseAzureGraphApi;

            var authority = string.Format(CultureInfo.InvariantCulture, _azureAdInstance, _azureTenant);
            var middlewares = new List<OwinMiddlewareRegistration>();

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            var openIdOptions = new OpenIdConnectAuthenticationOptions {
                ClientId = _azureClientId,
                Authority = authority,
                PostLogoutRedirectUri = logoutRedirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications() {
                    AuthorizationCodeReceived = (context) => {
                        var code = context.Code;
                        var credential = new ClientCredential(_azureClientId, azureAppKey);
                        _inMemoryCache.UserObjectId = context.AuthenticationTicket.Identity.FindFirst(Constants.AzureActiveDirectory.ObjectIdentifierKey).Value;
                        var authContext = new AuthenticationContext(authority, _inMemoryCache);
                        var result = authContext.AcquireTokenByAuthorizationCodeAsync(code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, _azureGraphApiUri).Result;

                        return Task.FromResult(0);
                    },
                    AuthenticationFailed = context => {
                        context.HandleResponse();
                        context.Response.Redirect(Constants.General.AuthenticationErrorUrl);

                        return Task.FromResult(0);
                    }
                }

            };

            if (azureWebSiteProtectionEnabled) {
                middlewares.Add(new OwinMiddlewareRegistration {
                    Priority = "9",
                    Configure = app => { app.SetDataProtectionProvider(new MachineKeyProtectionProvider()); }
                });
            }

            middlewares.Add(new OwinMiddlewareRegistration {
                Priority = Constants.General.OpenIdOwinMiddlewarePriority,
                Configure = app => {
                    app.UseOpenIdConnectAuthentication(openIdOptions);
                }
            });

            if (azureUseAzureGraphApi) {
                middlewares.Add(new OwinMiddlewareRegistration {
                    Priority = "11",
                    Configure = app => app.Use(async (context, next) => {
                        try {
                            if (_azureActiveDirectoryService.Token == null && _azureActiveDirectoryService.Token.IsEmpty()) {
                                RegenerateAzureGraphApiToken();
                            }
                            else {
                                if (DateTimeOffset.Compare(DateTimeOffset.UtcNow, _azureActiveDirectoryService.TokenExpiresOn) > 0) {
                                    RegenerateAzureGraphApiToken();
                                }
                            }
                        }
                        catch (Exception ex) {
                            Logger.Log(LogLevel.Error, ex, "An error occurred generating azure api credential {0}", ex.Message);
                        }

                        await next.Invoke();
                    })
                });
            }

            return middlewares;
        }

        private void RegenerateAzureGraphApiToken() {
            var result = GetAuthContext().AcquireTokenAsync(_azureGraphApiUri, GetClientCredential()).Result;

            _azureActiveDirectoryService.TokenExpiresOn = result.ExpiresOn;
            _azureActiveDirectoryService.Token = result.AccessToken;
            _azureActiveDirectoryService.AzureTenant = _azureTenant;
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
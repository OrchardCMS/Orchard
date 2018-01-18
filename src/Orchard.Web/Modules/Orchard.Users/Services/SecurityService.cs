using System;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class SecurityService : ISecurityService {

        private readonly ISiteService _siteService;

        public SecurityService(
            ISiteService siteService) {

            _siteService = siteService;
        }

        public TimeSpan GetAuthenticationCookieLifeSpan() {
            return _siteService
                .GetSiteSettings()
                .As<SecuritySettingsPart>()
                .AuthCookieLifeSpan;
        }
    }
}
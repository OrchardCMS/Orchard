using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.Settings;

namespace Orchard.OpenId.Providers {
    [OrchardFeature("Orchard.OpenId.Twitter")]
    public class Twitter : IOpenIdProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ISiteService _siteService;

        public Twitter(
            IWorkContextAccessor workContextAccessor,
            ISiteService siteService) {

            _workContextAccessor = workContextAccessor;
            _siteService = siteService;
        }

        public string AuthenticationType {
            get { return "Twitter"; }
        }

        public string Name {
            get { return "Twitter"; }
        }

        public string DisplayName {
            get { return "Twitter"; }
        }

        public bool IsValid {
            get { return IsProviderValid(); }
        }

        private bool IsProviderValid() {
            try {
                TwitterSettingsPart settings;
                ISite site;

                if (_siteService == null) { // happens if the provider was called early in the pipeline
                    var scope = _workContextAccessor.GetContext() ?? _workContextAccessor.CreateWorkContextScope().WorkContext;
                    site = scope.Resolve<ISiteService>().GetSiteSettings();
                }
                else {
                    site = _siteService.GetSiteSettings();
                }

                settings = site.As<TwitterSettingsPart>();

                return (settings != null && settings.IsValid);
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
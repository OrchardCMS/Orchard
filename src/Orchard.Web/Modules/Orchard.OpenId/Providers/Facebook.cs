using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.Settings;

namespace Orchard.OpenId.Providers {
    [OrchardFeature("Orchard.OpenId.Facebook")]
    public class Facebook : IOpenIdProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ISiteService _siteService;

        public Facebook(
            IWorkContextAccessor workContextAccessor,
            ISiteService siteService) {

            _workContextAccessor = workContextAccessor;
            _siteService = siteService;
        }

        public string AuthenticationType {
            get { return "Facebook"; }
        }

        public string Name {
            get { return "Facebook"; }
        }

        public string DisplayName {
            get { return "Facebook"; }
        }

        public bool IsValid {
            get { return IsProviderValid(); }
        }

        private bool IsProviderValid() {
            try {
                FacebookSettingsPart settings;
                ISite site;

                if (_siteService == null) { // happens if the provider was called early in the pipeline
                    var scope = _workContextAccessor.GetContext() ?? _workContextAccessor.CreateWorkContextScope().WorkContext;
                    site = scope.Resolve<ISiteService>().GetSiteSettings();
                }
                else {
                    site = _siteService.GetSiteSettings();
                }

                settings = site.As<FacebookSettingsPart>();

                return (settings != null && settings.IsValid);
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
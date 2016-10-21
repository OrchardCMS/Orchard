using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.Settings;

namespace Orchard.OpenId.Providers {
    [OrchardFeature("Orchard.OpenId.AzureActiveDirectory")]
    public class AzureActiveDirectory : IOpenIdProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ISiteService _siteService;

        public AzureActiveDirectory(
            IWorkContextAccessor workContextAccessor,
            ISiteService siteService) {

            _workContextAccessor = workContextAccessor;
            _siteService = siteService;
        }

        public string AuthenticationType {
            get { return "OpenIdConnect"; }
        }

        public string Name {
            get { return "AzureAD"; }
        }

        public string DisplayName {
            get { return "Azure Active Directory"; }
        }

        public bool IsValid {
            get { return IsProviderValid(); }
        }

        private bool IsProviderValid() {
            try {
                AzureActiveDirectorySettingsPart settings;
                ISite site;

                if (_siteService == null) { // happens if the provider was called early in the pipeline
                    var scope = _workContextAccessor.GetContext() ?? _workContextAccessor.CreateWorkContextScope().WorkContext;
                    site = scope.Resolve<ISiteService>().GetSiteSettings();
                }
                else {
                    site = _siteService.GetSiteSettings();
                }

                settings = site.As<AzureActiveDirectorySettingsPart>();

                return (settings != null && settings.IsValid);
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
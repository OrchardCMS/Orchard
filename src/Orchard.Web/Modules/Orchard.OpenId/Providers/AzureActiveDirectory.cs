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

        public AzureActiveDirectory(
            IWorkContextAccessor workContextAccessor) {

            _workContextAccessor = workContextAccessor;
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

                // TODO: Revise code after issue https://github.com/OrchardCMS/Orchard/issues/7362 has been solved
                var scope =
                    _workContextAccessor.GetContext() ?? // happens if the provider was called early in the pipeline
                    _workContextAccessor.CreateWorkContextScope().WorkContext;

                site = scope.Resolve<ISiteService>().GetSiteSettings();

                settings = site.As<AzureActiveDirectorySettingsPart>();

                return (settings != null && settings.IsValid);
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
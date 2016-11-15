using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.Settings;

namespace Orchard.OpenId.Providers {
    [OrchardFeature("Orchard.OpenId.ActiveDirectoryFederationServices")]
    public class ActiveDirectoryFederationServices : IOpenIdProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public ActiveDirectoryFederationServices(
            IWorkContextAccessor workContextAccessor) {

            _workContextAccessor = workContextAccessor;
        }

        public string AuthenticationType {
            get { return "OpenIdConnect"; }
        }

        public string Name {
            get { return "ADFS"; }
        }

        public string DisplayName {
            get { return "Active Directory Federation Services"; }
        }

        public bool IsValid {
            get { return IsProviderValid(); }
        }

        private bool IsProviderValid() {
            try {
                ActiveDirectoryFederationServicesSettingsPart settings;
                ISite site;

                // TODO: Revise code after issue https://github.com/OrchardCMS/Orchard/issues/7362 has been solved
                var scope =
                    _workContextAccessor.GetContext() ?? // happens if the provider was called early in the pipeline
                    _workContextAccessor.CreateWorkContextScope().WorkContext;

                site = scope.Resolve<ISiteService>().GetSiteSettings();

                settings = site.As<ActiveDirectoryFederationServicesSettingsPart>();

                return (settings != null && settings.IsValid);
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
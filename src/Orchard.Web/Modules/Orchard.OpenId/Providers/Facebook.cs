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

        public Facebook(
            IWorkContextAccessor workContextAccessor) {

            _workContextAccessor = workContextAccessor;
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

                var scope = _workContextAccessor.GetContext();

                site = scope.Resolve<ISiteService>().GetSiteSettings();
                settings = site.As<FacebookSettingsPart>();

                return (settings != null && settings.IsValid());
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
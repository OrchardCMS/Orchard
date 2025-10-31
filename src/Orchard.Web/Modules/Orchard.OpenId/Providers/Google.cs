using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.Settings;

namespace Orchard.OpenId.Providers
{
    [OrchardFeature("Orchard.OpenId.Google")]
    public class Google : IOpenIdProvider
    {
        private readonly IWorkContextAccessor _workContextAccessor;

        public Google(
            IWorkContextAccessor workContextAccessor,
            ISiteService siteService)
        {

            _workContextAccessor = workContextAccessor;
        }

        public string AuthenticationType => "Google";

        public string Name => "Google";

        public string DisplayName => "Google";

        public bool IsValid => IsProviderValid();

        private bool IsProviderValid()
        {
            try
            {
                GoogleSettingsPart settings;
                ISite site;

                var scope = _workContextAccessor.GetContext();

                site = scope.Resolve<ISiteService>().GetSiteSettings();
                settings = site.As<GoogleSettingsPart>();

                return settings != null && settings.IsValid();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
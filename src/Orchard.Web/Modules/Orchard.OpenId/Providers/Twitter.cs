using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.Settings;

namespace Orchard.OpenId.Providers
{
    [OrchardFeature("Orchard.OpenId.Twitter")]
    public class Twitter : IOpenIdProvider
    {
        private readonly IWorkContextAccessor _workContextAccessor;

        public Twitter(
            IWorkContextAccessor workContextAccessor)
        {

            _workContextAccessor = workContextAccessor;
        }

        public string AuthenticationType => "Twitter";

        public string Name => "Twitter";

        public string DisplayName => "Twitter";

        public bool IsValid => IsProviderValid();

        private bool IsProviderValid()
        {
            try
            {
                TwitterSettingsPart settings;
                ISite site;

                var scope = _workContextAccessor.GetContext();

                site = scope.Resolve<ISiteService>().GetSiteSettings();
                settings = site.As<TwitterSettingsPart>();

                return (settings != null && settings.IsValid());
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
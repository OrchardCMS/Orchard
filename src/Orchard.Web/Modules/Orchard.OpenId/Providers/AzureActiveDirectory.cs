using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.Settings;

namespace Orchard.OpenId.Providers
{
    [OrchardFeature("Orchard.OpenId.AzureActiveDirectory")]
    public class AzureActiveDirectory : IOpenIdProvider
    {
        private readonly IWorkContextAccessor _workContextAccessor;

        public AzureActiveDirectory(
            IWorkContextAccessor workContextAccessor)
        {

            _workContextAccessor = workContextAccessor;
        }

        public string AuthenticationType => "OpenIdConnect";

        public string Name => "AzureAD";

        public string DisplayName => "Azure Active Directory";

        public bool IsValid => IsProviderValid();

        private bool IsProviderValid()
        {
            try
            {
                AzureActiveDirectorySettingsPart settings;
                ISite site;

                var scope = _workContextAccessor.GetContext();

                site = scope.Resolve<ISiteService>().GetSiteSettings();
                settings = site.As<AzureActiveDirectorySettingsPart>();

                return (settings != null && settings.IsValid());
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
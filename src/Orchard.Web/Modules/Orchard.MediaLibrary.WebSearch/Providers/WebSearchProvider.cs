using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.WebSearch.Models;
using Orchard.Settings;

namespace Orchard.MediaLibrary.WebSearch.Providers {
    public interface IWebSearchProvider : IDependency {
        string Name { get; }
        bool IsValid { get; } // Do we need this?
        string ApiKey { get; }
    }

    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Bing")]
    public class BingWebSearchProvider : IWebSearchProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public BingWebSearchProvider(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public string Name => "Bing";

        public bool IsValid => !String.IsNullOrEmpty(GetApiKey());

        public string ApiKey => GetApiKey();

        private string GetApiKey() {
            try {
                BingWebSearchSettingsPart settings;
                ISite site;

                var scope = _workContextAccessor.GetContext();

                site = scope.Resolve<ISiteService>().GetSiteSettings();
                settings = site.As<BingWebSearchSettingsPart>();

                return settings.ApiKey;
            }
            catch (Exception) {
                return null;
            }
        }
    }

    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Pixabay")]
    public class PixabayWebSearchProvider : IWebSearchProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public PixabayWebSearchProvider(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public string Name => "Pixabay";

        public bool IsValid => !String.IsNullOrEmpty(GetApiKey());

        public string ApiKey => GetApiKey();

        private string GetApiKey() {
            try {
                PixabayWebSearchSettingsPart settings;
                ISite site;

                var scope = _workContextAccessor.GetContext();

                site = scope.Resolve<ISiteService>().GetSiteSettings();
                settings = site.As<PixabayWebSearchSettingsPart>();

                return settings.ApiKey;
            }
            catch (Exception) {
                return null;
            }
        }
    }
}
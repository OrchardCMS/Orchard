using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.WebSearch.Models;
using Orchard.MediaLibrary.WebSearch.ViewModels;
using Orchard.Services;
using Orchard.Settings;
using RestEase;

namespace Orchard.MediaLibrary.WebSearch.Providers {
    public interface IWebSearchProvider : IDependency {
        string Name { get; }
        bool IsValid { get; } // Do we need this?
        string ApiKey { get; }
        List<WebSearchResult> GetImages(string query);
    }

    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Bing")]
    public class BingWebSearchProvider : IWebSearchProvider {
        private const string BingBaseUrl = "https://api.cognitive.microsoft.com";

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IJsonConverter _jsonConverter;

        public BingWebSearchProvider(IWorkContextAccessor workContextAccessor, IJsonConverter jsonConverter) {
            _workContextAccessor = workContextAccessor;
            _jsonConverter = jsonConverter;
        }

        public string Name => "Bing";

        public bool IsValid => !String.IsNullOrEmpty(GetApiKey());

        public string ApiKey => GetApiKey();

        public List<WebSearchResult> GetImages(string query) {
            var client = RestClient.For<IBingApi>(BingBaseUrl);

            var ratingResults = client.GetImagesAsync(ApiKey, query);
            var apiResult = _jsonConverter.Deserialize<dynamic>(ratingResults.Result);
            var webSearchResult = new List<WebSearchResult>();

            foreach (var hit in apiResult.value) {
                String imageSize = hit.contentSize;
                webSearchResult.Add(new WebSearchResult() {
                    ThumbnailUrl = hit.thumbnailUrl,
                    Width = hit.width,
                    Height = hit.height,
                    ImageUrl = hit.contentUrl,
                    Size = int.Parse(imageSize.Substring(0, imageSize.Length - 2))

                });
            }

            return webSearchResult;
        }

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
        private const string PixabayBaseUrl = "https://pixabay.com";

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IJsonConverter _jsonConverter;

        public PixabayWebSearchProvider(IWorkContextAccessor workContextAccessor, IJsonConverter jsonConverter) {
            _workContextAccessor = workContextAccessor;
            _jsonConverter = jsonConverter;
        }

        public string Name => "Pixabay";

        public bool IsValid => !String.IsNullOrEmpty(GetApiKey());

        public string ApiKey => GetApiKey();

        public List<WebSearchResult> GetImages(string query) {            
            var client = RestClient.For<IPixabayApi>(PixabayBaseUrl);

            var ratingResults = client.GetImagesAsync(ApiKey, query);
            var pixabayApiResult = _jsonConverter.Deserialize<dynamic>(ratingResults.Result);
            var webSearchResult = new List<WebSearchResult>();

            foreach (var hit in pixabayApiResult.hits) {
                webSearchResult.Add(new WebSearchResult() {
                    ThumbnailUrl = hit.previewURL,
                    Width = hit.imageWidth,
                    Height = hit.imageHeight,
                    ImageUrl = hit.largeImageURL,
                    Size = hit.imageSize

                });
            }

            return webSearchResult;
        }

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
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.WebSearch.Controllers.Api;
using Orchard.MediaLibrary.WebSearch.Models;
using Orchard.MediaLibrary.WebSearch.ViewModels;
using Orchard.Services;
using Orchard.Settings;
using RestEase;

namespace Orchard.MediaLibrary.WebSearch.Providers {
    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Google")]
    public class GoogleWebSearchProvider : IWebSearchProvider {
        private const string GoogleBaseUrl = "https://www.googleapis.com";

        private readonly ISiteService _siteService;
        private readonly IJsonConverter _jsonConverter;

        public GoogleWebSearchProvider(ISiteService siteService, IJsonConverter jsonConverter) {
            _siteService = siteService;
            _jsonConverter = jsonConverter;
        }

        private GoogleWebSearchSettingsPart _settings =>
           _siteService.GetSiteSettings().As<GoogleWebSearchSettingsPart>();

        public IWebSearchSettings Settings => _settings;

        public string Name => "Google";

        public IEnumerable<WebSearchResult> GetImages(string query) {
            var client = RestClient.For<IGoogleApi>(GoogleBaseUrl);

            var apiResponse = client.GetImagesAsync(this.GetApiKey(), _settings.SearchEngineId, query);
            var apiResult = _jsonConverter.Deserialize<dynamic>(apiResponse.Result);
            var webSearchResult = new List<WebSearchResult>();

            foreach (var hit in apiResult.items) {
                string imageSize = hit.contentSize;

                webSearchResult.Add(new WebSearchResult() {
                    ThumbnailUrl = hit.image.thumbnailLink,
                    Width = hit.image.width,
                    Height = hit.image.height,
                    ImageUrl = hit.link,
                    Size = hit.image.byteSize,
                    PageUrl = hit.image.contextLink
                });
            }

            return webSearchResult.Any() ? webSearchResult : Enumerable.Empty<WebSearchResult>();
        }
    }
}
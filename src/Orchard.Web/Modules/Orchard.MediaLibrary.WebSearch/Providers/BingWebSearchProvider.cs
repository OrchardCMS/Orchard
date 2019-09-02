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
    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Bing")]
    public class BingWebSearchProvider : IWebSearchProvider {
        private const string BingBaseUrl = "https://api.cognitive.microsoft.com";

        private readonly ISiteService _siteService;
        private readonly IJsonConverter _jsonConverter;

        public BingWebSearchProvider(ISiteService siteService, IJsonConverter jsonConverter) {
            _siteService = siteService;
            _jsonConverter = jsonConverter;
        }

        private BingWebSearchSettingsPart _settings =>
           _siteService.GetSiteSettings().As<BingWebSearchSettingsPart>();

        public IWebSearchSettings Settings => _settings;

        public string Name => "Bing";

        public IEnumerable<WebSearchResult> GetImages(string query) {
            var client = RestClient.For<IBingApi>(BingBaseUrl);

            var apiResponse = client.GetImagesAsync(this.GetApiKey(), query);
            var apiResult = _jsonConverter.Deserialize<dynamic>(apiResponse.Result);
            var webSearchResult = new List<WebSearchResult>();

            foreach (var hit in apiResult.value) {
                string imageSize = hit.contentSize;

                webSearchResult.Add(new WebSearchResult() {
                    ThumbnailUrl = hit.thumbnailUrl,
                    Width = hit.width,
                    Height = hit.height,
                    ImageUrl = hit.contentUrl,
                    Size = int.Parse(imageSize.Substring(0, imageSize.Length - 2)),
                    PageUrl = hit.hostPageUrl
                });
            }

            return webSearchResult.Any() ? webSearchResult : Enumerable.Empty<WebSearchResult>();
        }
    }
}
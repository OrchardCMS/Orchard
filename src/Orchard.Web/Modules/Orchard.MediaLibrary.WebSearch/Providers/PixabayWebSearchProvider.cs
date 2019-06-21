using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.WebSearch.Models;
using Orchard.MediaLibrary.WebSearch.ViewModels;
using Orchard.Services;
using Orchard.Settings;
using RestEase;

namespace Orchard.MediaLibrary.WebSearch.Providers {
    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Pixabay")]
    public class PixabayWebSearchProvider : WebSearchProviderBase {
        private const string PixabayBaseUrl = "https://pixabay.com";

        private readonly ISiteService _siteService;
        private readonly IJsonConverter _jsonConverter;

        public PixabayWebSearchProvider(ISiteService siteService, IJsonConverter jsonConverter) {
            _siteService = siteService;
            _jsonConverter = jsonConverter;
        }

        public override string Name => "Pixabay";

        private PixabayWebSearchSettingsPart _settings =>
           _siteService.GetSiteSettings().As<PixabayWebSearchSettingsPart>();

        public override IWebSearchSettings Settings => _settings;

        public override IEnumerable<WebSearchResult> GetImages(string query) {
            var client = RestClient.For<IPixabayApi>(PixabayBaseUrl);

            var apiResponse = client.GetImagesAsync(ApiKey, query);
            var apiResult = _jsonConverter.Deserialize<dynamic>(apiResponse.Result);
            var webSearchResult = new List<WebSearchResult>();

            foreach (var hit in apiResult.hits) {
                webSearchResult.Add(new WebSearchResult() {
                    ThumbnailUrl = hit.previewURL,
                    Width = hit.imageWidth,
                    Height = hit.imageHeight,
                    ImageUrl = hit.largeImageURL,
                    Size = hit.imageSize,
                    PageUrl = hit.pageURL
                });
            }

            return webSearchResult.Any() ? webSearchResult : Enumerable.Empty<WebSearchResult>();
        }
    }
}
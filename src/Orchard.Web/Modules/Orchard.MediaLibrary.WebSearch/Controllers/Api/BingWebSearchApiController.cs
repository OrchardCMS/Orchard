using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.WebSearch.Providers;
using Orchard.Security;
using RestEase;

namespace Orchard.MediaLibrary.WebSearch.Controllers.Api {
    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Bing")]
    public class BingWebSearchApiController : ApiController {
        private const string BingBaseUrl = "https://api.cognitive.microsoft.com";

        private readonly IAuthorizer _authorizer;
        private readonly IEnumerable<IWebSearchProvider> _wsp;

        public BingWebSearchApiController(IAuthorizer authorizer, IEnumerable<IWebSearchProvider> wsp) {
            _authorizer = authorizer;
            _wsp = wsp;
        }

        [HttpGet]
        public HttpResponseMessage GetImages(string query) {
            if (!_authorizer.Authorize(Permissions.ManageMediaContent)) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            var bingApiKey = _wsp.FirstOrDefault(provider => provider.Name == "Bing")?.ApiKey;
            if (bingApiKey == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var client = RestClient.For<IBingApi>(BingBaseUrl);

            var ratingResults = client.GetImagesAsync(bingApiKey, query);
            return ratingResults.Result;
        }
    }
}
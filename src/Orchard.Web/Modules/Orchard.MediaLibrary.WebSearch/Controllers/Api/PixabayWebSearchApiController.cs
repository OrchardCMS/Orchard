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
    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Pixabay")]
    public class PixabayWebSearchApiController : ApiController {
        private const string PixabayBaseUrl = "https://pixabay.com";

        private readonly IAuthorizer _authorizer;
        private readonly IEnumerable<IWebSearchProvider> _wsp;

        public PixabayWebSearchApiController(IAuthorizer authorizer, IEnumerable<IWebSearchProvider> wsp) {
            _authorizer = authorizer;
            _wsp = wsp;
        }

        //IHttpactionResult? something hacky with string
        [HttpGet]
        public HttpResponseMessage GetImages(string query) {
            if (!_authorizer.Authorize(Permissions.ManageMediaContent)) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            var pixabayApiKey = _wsp.FirstOrDefault(provider => provider.Name == "Pixabay")?.ApiKey;
            if (pixabayApiKey == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var client = RestClient.For<IPixabayApi>(PixabayBaseUrl);

            var ratingResults = client.GetImagesAsync(pixabayApiKey, query);
            return ratingResults.Result;
        }
    }
}
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.ImportExport.ApiControllers {
    [OrchardFeature("Orchard.Deployment")]
    public abstract class BaseApiController : ApiController {
        protected readonly ISigningService _signingService;
        protected readonly IAuthenticationService _authenticationService;
        protected readonly IClock _clock;

        protected BaseApiController(ISigningService signingService,
            IAuthenticationService authenticationService,
            IClock clock) {
            _signingService = signingService;
            _authenticationService = authenticationService;
            _clock = clock;
        }

        protected HttpResponseMessage CreateSignedResponse(string content) {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            };

            if (!string.IsNullOrWhiteSpace(content)) {
                var user = _authenticationService.GetAuthenticatedUser();
                var timestamp = _clock.UtcNow.ToString(_signingService.TimestampFormat);
                response.Content.Headers.Add(_signingService.TimestampHeaderName, timestamp);
                response.Content.Headers.Add(_signingService.ContentHashHeaderName,
                    _signingService.SignContent(content, timestamp, user.As<DeploymentUserPart>().PrivateApiKey));
            }

            return response;
        }

        protected bool ValidateContent(string content, HttpRequestHeaders headers) {
            if (string.IsNullOrWhiteSpace(content))
                return true;

            string timestamp = GetHttpRequestHeader(headers, _signingService.TimestampHeaderName);
            string contentHash = HttpUtility.UrlDecode(GetHttpRequestHeader(headers, _signingService.ContentHashHeaderName));

            var user = _authenticationService.GetAuthenticatedUser();
            if (user != null && user.Is<DeploymentUserPart>()) {
                return _signingService.ValidateContent(content, timestamp, user.As<DeploymentUserPart>().PrivateApiKey, contentHash);
            }
            return false;
        }

        protected static string GetHttpRequestHeader(HttpHeaders headers, string headerName)
        {
            if (!headers.Contains(headerName))
                return string.Empty;

            return headers
                .GetValues(headerName)
                .SingleOrDefault();
        }
    }
}

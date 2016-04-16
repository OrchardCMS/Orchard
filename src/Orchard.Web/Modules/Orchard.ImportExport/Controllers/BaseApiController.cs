using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.ImportExport.Controllers {
    [OrchardFeature("Orchard.Deployment")]
    public abstract class BaseApiController : Controller {
        protected readonly ISigningService _signingService;
        protected readonly IAuthenticationService _authenticationService;
        protected readonly IClock _clock;

        protected BaseApiController(
            ISigningService signingService,
            IAuthenticationService authenticationService,
            IClock clock
            ) {
            _signingService = signingService;
            _authenticationService = authenticationService;
            _clock = clock;
        }

        protected ActionResult CreateSignedResponse(string content) {
            if (string.IsNullOrWhiteSpace(content)) return Content("");

            var user = _authenticationService.GetAuthenticatedUser();
            var timestamp = _clock.UtcNow.ToString(_signingService.TimestampFormat);
            Response.Headers.Add(_signingService.TimestampHeaderName, timestamp);
            Response.Headers.Add(_signingService.ContentHashHeaderName,
                _signingService.SignContent(content, timestamp, user.As<DeploymentUserPart>().PrivateApiKey));

            return Content(content);
        }

        protected FilePathResult CreateSignedResponse(FilePathResult result) {
            var user = _authenticationService.GetAuthenticatedUser();
            var timestamp = _clock.UtcNow.ToString(_signingService.TimestampFormat);
            Response.Headers.Add(_signingService.TimestampHeaderName, timestamp);
            var package = System.IO.File.ReadAllBytes(result.FileName);
            Response.Headers.Add(_signingService.ContentHashHeaderName,
                _signingService.SignContent(package, timestamp, user.As<DeploymentUserPart>().PrivateApiKey));

            return result;
        }

        protected bool ValidateContent(string content, NameValueCollection headers) {
            if (string.IsNullOrWhiteSpace(content)) return true;

            var timestamp = GetHttpRequestHeader(headers, _signingService.TimestampHeaderName);
            var contentHash = HttpUtility.UrlDecode(GetHttpRequestHeader(headers, _signingService.ContentHashHeaderName));

            var user = _authenticationService.GetAuthenticatedUser();
            if (user != null && user.Is<DeploymentUserPart>()) {
                return _signingService.ValidateContent(content, timestamp, user.As<DeploymentUserPart>().PrivateApiKey, contentHash);
            }
            return false;
        }

        protected bool ValidateContent(Stream content, NameValueCollection headers) {
            if (content == null) return true;

            var timestamp = GetHttpRequestHeader(headers, _signingService.TimestampHeaderName);
            var contentHash = HttpUtility.UrlDecode(GetHttpRequestHeader(headers, _signingService.ContentHashHeaderName));

            var user = _authenticationService.GetAuthenticatedUser();
            if (user != null && user.Is<DeploymentUserPart>()) {
                return _signingService.ValidateContent(content, timestamp, user.As<DeploymentUserPart>().PrivateApiKey, contentHash);
            }
            return false;
        }

        protected static string GetHttpRequestHeader(NameValueCollection headers, string headerName) {
            return headers[headerName] ?? "";
        }
    }
}

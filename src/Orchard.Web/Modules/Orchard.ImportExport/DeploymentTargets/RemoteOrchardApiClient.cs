using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Services;

namespace Orchard.ImportExport.DeploymentTargets {
    public class RemoteOrchardApiClient {
        private const string ApiBasePath = "api/deployment/";
        private readonly RemoteOrchardDeploymentPart _config;
        private readonly ISigningService _signingService;
        private readonly IClock _clock;

        public RemoteOrchardApiClient(RemoteOrchardDeploymentPart config, ISigningService signingService, IClock clock) {
            _config = config;
            _signingService = signingService;
            _clock = clock;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string Get(string url) {
            var fullyQualifiedUri = BuildUri(url);
            var timestamp = _clock.UtcNow.ToString(_signingService.TimestampFormat);
            var signature = _signingService.SignRequest("GET", timestamp, fullyQualifiedUri.AbsolutePath, _config.PrivateApiKey);

            using (var webClient = CreateWebClient(_config.UserName, timestamp, signature, null)) {
                using (var stream = webClient.OpenRead(fullyQualifiedUri.ToString())) {
                    if (stream == null) {
                        throw new WebException("Deployment API did not return a valid stream.");
                    }
                    using (var reader = new StreamReader(stream)) {
                        var json = reader.ReadToEnd();
                        stream.Close();
                        reader.Close();

                        if (!ResponseIsValid(json,
                            webClient.ResponseHeaders[_signingService.TimestampHeaderName],
                            webClient.ResponseHeaders[_signingService.ContentHashHeaderName])) {
                            throw new WebException("Deployment API response does not contain a valid hash");
                        }

                        return json;
                    }
                }
            }
        }

        public string Post(string url, string data, string contentType = "application/json") {
            var fullyQualifiedUri = BuildUri(url);
            var timestamp = _clock.UtcNow.ToString(_signingService.TimestampFormat);
            var signature = _signingService.SignRequest("POST", timestamp, fullyQualifiedUri.AbsolutePath, _config.PrivateApiKey);
            var requestContentHash = _signingService.SignContent(data, timestamp, _config.PrivateApiKey);

            using (var webClient = CreateWebClient(_config.UserName, timestamp, signature, requestContentHash)) {
                webClient.Headers["Content-Type"] = contentType;
                var result = webClient.UploadString(fullyQualifiedUri.ToString(), data);

                if (!ResponseIsValid(result,
                    webClient.ResponseHeaders[_signingService.TimestampHeaderName],
                    webClient.ResponseHeaders[_signingService.ContentHashHeaderName])) {
                    throw new WebException("Deployment API response does not contain a valid hash");
                }
                return result;
            }
        }

        private Uri BuildUri(string relativeUrl) {
            relativeUrl = ApiBasePath + relativeUrl;
            var baseUri = new Uri(_config.BaseUrl);
            return new Uri(baseUri, relativeUrl);
        }

        private WebClient CreateWebClient(string username, string timestamp, string signature, string contentHash) {
            var webClient = new WebClient {Encoding = Encoding.UTF8};
            webClient.Headers[_signingService.AuthenticationHeaderName] =
                string.Format("{0}:{1}", username, HttpUtility.UrlEncode(signature));
            webClient.Headers[_signingService.TimestampHeaderName] = timestamp;
            if (!string.IsNullOrEmpty(contentHash)) {
                webClient.Headers[_signingService.ContentHashHeaderName] = HttpUtility.UrlEncode(contentHash);
            }
            return webClient;
        }

        private bool ResponseIsValid(string result, string timestamp, string contentHash) {
            return (string.IsNullOrWhiteSpace(result) || _signingService.ValidateContent(result, timestamp, _config.PrivateApiKey, contentHash));
        }
    }
}
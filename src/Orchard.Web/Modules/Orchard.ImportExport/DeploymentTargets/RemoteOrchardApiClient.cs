using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Services;

namespace Orchard.ImportExport.DeploymentTargets {
    public class RemoteOrchardApiClient {
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
                        throw new WebException(T("Deployment API did not return a valid stream.").Text);
                    }
                    if (!ResponseIsValid(stream,
                        webClient.ResponseHeaders[_signingService.TimestampHeaderName],
                        webClient.ResponseHeaders[_signingService.ContentHashHeaderName])) {
                        throw new WebException(T("Deployment API response does not contain a valid hash").Text);
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream)) {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public WebResponse Post(string url, Stream data) {
            var fullyQualifiedUri = BuildUri(url);
            var timestamp = _clock.UtcNow.ToString(_signingService.TimestampFormat);
            var signature = _signingService.SignRequest("POST", timestamp, fullyQualifiedUri.AbsolutePath, _config.PrivateApiKey);
            var requestContentHash = _signingService.SignContent(data, timestamp, _config.PrivateApiKey);

            var request = CreateWebRequest(fullyQualifiedUri.ToString(), _config.UserName, timestamp, signature, requestContentHash);
            request.ContentType = "multipart/form-data";
            request.Method = "POST";
            var boundary = "\r\n--------------------------" + Guid.NewGuid().ToString("n");
            var boundaryBytes = Encoding.UTF8.GetBytes(boundary + "\r\n");

            using(var requestStream = request.GetRequestStream()) {
                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                var fileHeader = Encoding.UTF8.GetBytes(
                    "Content-Disposition: form-data; name=\"export\"; filename=\"export.nupkg\"\r\nContent-Type: application/zip");
                requestStream.Write(fileHeader, 0, fileHeader.Length);
                data.CopyTo(requestStream);
                var footerBytes = Encoding.UTF8.GetBytes(boundary + "--\r\n");
                requestStream.Write(footerBytes, 0, footerBytes.Length);
            }
            var response = request.GetResponse();

            if (!ResponseIsValid(response.GetResponseStream(),
                response.Headers[_signingService.TimestampHeaderName],
                response.Headers[_signingService.ContentHashHeaderName])) {
                throw new WebException(T("Deployment API response does not contain a valid hash").Text);
            }
            return response;
        }

        public WebResponse Post(string url, string data, string contentType = "application/json") {
            var fullyQualifiedUri = BuildUri(url);
            var timestamp = _clock.UtcNow.ToString(_signingService.TimestampFormat);
            var signature = _signingService.SignRequest("POST", timestamp, fullyQualifiedUri.AbsolutePath, _config.PrivateApiKey);
            var requestContentHash = _signingService.SignContent(data, timestamp, _config.PrivateApiKey);

            var request = CreateWebRequest(fullyQualifiedUri.ToString(), _config.UserName, timestamp, signature, requestContentHash);
            request.Method = "POST";
            request.ContentType = contentType;
            var dataBytes = Encoding.UTF8.GetBytes(data);
            request.GetRequestStream().Write(dataBytes, 0, dataBytes.Length);
            var response = request.GetResponse();

            // Skip response validation if it's empty
            if (response.ContentLength == 0) return response;
            // Otherwise check contents and timestamp against signature from headers.
            if (!ResponseIsValid(response.GetResponseStream(),
                response.Headers[_signingService.TimestampHeaderName],
                response.Headers[_signingService.ContentHashHeaderName])) {
                throw new WebException(T("Deployment API response does not contain a valid hash").Text);
            }
            return response;
        }

        private Uri BuildUri(string relativeUrl) {
            var baseUri = new Uri(_config.BaseUrl);
            return new Uri(baseUri, relativeUrl);
        }

        private WebClient CreateWebClient(string username, string timestamp, string signature, string contentHash) {
            var webClient = new WebClient {Encoding = Encoding.UTF8};
            webClient.Headers[_signingService.AuthenticationHeaderName] =
                username + ":" + HttpUtility.UrlEncode(signature);
            webClient.Headers[_signingService.TimestampHeaderName] = timestamp;
            if (!string.IsNullOrEmpty(contentHash)) {
                webClient.Headers[_signingService.ContentHashHeaderName] = HttpUtility.UrlEncode(contentHash);
            }
            return webClient;
        }

        private WebRequest CreateWebRequest(string url, string username, string timestamp, string signature, string contentHash) {
            var request = WebRequest.Create(url);
            request.Headers[_signingService.AuthenticationHeaderName] =
                username + ":" + HttpUtility.UrlEncode(signature);
            request.Headers[_signingService.TimestampHeaderName] = timestamp;
            if (!string.IsNullOrEmpty(contentHash)) {
                request.Headers[_signingService.ContentHashHeaderName] = HttpUtility.UrlEncode(contentHash);
            }
            return request;
        }

        private bool ResponseIsValid(Stream result, string timestamp, string contentHash) {
            return (!result.CanRead || _signingService.ValidateContent(result, timestamp, _config.PrivateApiKey, contentHash));
        }
    }
}
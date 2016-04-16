using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Orchard.FileSystems.AppData;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Services;

namespace Orchard.ImportExport.DeploymentTargets {
    public class RemoteOrchardApiClient {
        private readonly RemoteOrchardDeploymentPart _config;
        private readonly ISigningService _signingService;
        private readonly IClock _clock;
        private readonly IAppDataFolder _appData;

        public RemoteOrchardApiClient(
            RemoteOrchardDeploymentPart config,
            ISigningService signingService,
            IClock clock,
            IAppDataFolder appData
            ) {
            _config = config;
            _signingService = signingService;
            _clock = clock;
            _appData = appData;
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
                    // Response stream doesn't support seeking, need to copy it locally to a temp file.
                    var tempPath = Path.Combine("temp", Guid.NewGuid().ToString("n"));
                    using (var tempStream = _appData.CreateFile(tempPath)) {
                        try {
                            stream.CopyTo(tempStream);
                            tempStream.Seek(0, SeekOrigin.Begin);
                            if (!ResponseIsValid(tempStream,
                                webClient.ResponseHeaders[_signingService.TimestampHeaderName],
                                webClient.ResponseHeaders[_signingService.ContentHashHeaderName])) {
                                throw new WebException(T("Deployment API response does not contain a valid hash").Text);
                            }
                            tempStream.Seek(0, SeekOrigin.Begin);
                            using (var reader = new StreamReader(tempStream)) {
                                return reader.ReadToEnd();
                            }
                        }
                        finally {
                            _appData.DeleteFile(tempPath);
                        }
                    }
                }
            }
        }

        public void PostStream(string url, Stream data) {
            var fullyQualifiedUri = BuildUri(url);
            var timestamp = _clock.UtcNow.ToString(_signingService.TimestampFormat);
            var signature = _signingService.SignRequest("POST", timestamp, fullyQualifiedUri.AbsolutePath, _config.PrivateApiKey);
            var requestContentHash = _signingService.SignContent(data, timestamp, _config.PrivateApiKey);

            var request = CreateWebRequest(fullyQualifiedUri.ToString(), _config.UserName, timestamp, signature, requestContentHash);
            var boundary = "--------------------------" + Guid.NewGuid().ToString("n");
            request.ContentType = "multipart/form-data, boundary=" + boundary;
            request.Method = "POST";
            var headerBytes = Encoding.UTF8.GetBytes(
                "--" + boundary + "\r\n" +
                "Content-Disposition: form-data; name=\"export\"; filename=\"export.nupkg\"\r\n" +
                "Content-Type: application/zip\r\n\r\n");
            var boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            var length = headerBytes.Length + data.Length + boundaryBytes.Length;
            request.ContentLength = length;

            using(var requestStream = request.GetRequestStream()) {
                requestStream.Write(headerBytes, 0, headerBytes.Length);
                data.CopyTo(requestStream);
                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            }

            request.GetResponse();
        }

        public string PostForFile(string url, string data, string targetFilePath, string contentType = "application/json") {
            var response = GetPostResponse(url, data, contentType);
            var deploymentId = Guid.NewGuid().ToString("n");
            var filename = deploymentId + (response.Headers["content-type"] == "text/xml" ? ".xml" : ".nupkg");
            var path = _appData.Combine(targetFilePath, filename);
            using (var file = _appData.CreateFile(path)) {
                try {
                    response.GetResponseStream().CopyTo(file);
                    file.Seek(0, SeekOrigin.Begin);
                    if (!ResponseIsValid(file,
                        response.Headers[_signingService.TimestampHeaderName],
                        response.Headers[_signingService.ContentHashHeaderName])) {
                        throw new WebException(T("Deployment API response does not contain a valid hash").Text);
                    }
                    return _appData.MapPath(path);
                }
                catch(Exception) {
                    _appData.DeleteFile(path);
                    throw;
                }
            }
        }

        public void Post(string url, string data, string contentType = "application/json") {
            GetPostResponse(url, data, contentType);
        }

        private WebResponse GetPostResponse(string url, string data, string contentType) {
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
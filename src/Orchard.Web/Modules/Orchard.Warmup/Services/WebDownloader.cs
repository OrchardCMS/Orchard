using System;
using System.IO;
using System.Net;
using Orchard.Logging;

namespace Orchard.Warmup.Services {
    public class WebDownloader : IWebDownloader {
        public WebDownloader() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public DownloadResult Download(string url) {
            if(String.IsNullOrWhiteSpace(url)) {
                return null;
            }

            try {
                var request = WebRequest.Create(url) as HttpWebRequest;
                if (request != null) {
                    using (var response = request.GetResponse() as HttpWebResponse) {
                        if (response != null) {
                            using (var stream = response.GetResponseStream()) {
                                if (stream != null) {
                                    using (var sr = new StreamReader(stream)) {
                                        return new DownloadResult {Content = sr.ReadToEnd(), StatusCode = response.StatusCode};
                                    }
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (WebException e) {
                if(e.Response as HttpWebResponse != null) {
                    return new DownloadResult { StatusCode = ((HttpWebResponse)e.Response).StatusCode };
                }

                return null;
            }
            catch(Exception e) {
                Logger.Error(e, "An error occured while downloading url: {0}", url);
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using Orchard.Specs.Util;

namespace Orchard.Specs.Hosting {
    public static class RequestExtensions {
        public static RequestDetails SendRequest(this WebHost webHost, string urlPath, IDictionary<string, IEnumerable<string>> postData, string requestMethod = null) {

            var physicalPath = Bleroy.FluentPath.Path.Get(webHost.PhysicalDirectory);

            bool isHomepage = urlPath == "/";

            if (!isHomepage)
                urlPath = StripVDir(urlPath, webHost.VirtualDirectory);

            var details = new RequestDetails {
                HostName = webHost.HostName,
                UrlPath = urlPath.Replace('\\', '/'),
            };

            int queryIndex = urlPath.IndexOf('?');
            if (queryIndex >= 0) {
                details.UrlPath = urlPath.Substring(0, queryIndex).Replace('\\', '/');
                details.Query = urlPath.Substring(queryIndex + 1);
            }

            var physicalFilePath = physicalPath.Combine(details.UrlPath.TrimStart('/', '\\'));
            details.Page = (isHomepage ? "" : physicalFilePath.GetRelativePath(physicalPath).ToString());

            if (!File.Exists(physicalFilePath))
                details.Page = details.Page.Replace('\\', '/');

            if (!string.IsNullOrEmpty(webHost.Cookies)) {
                details.RequestHeaders.Add("Cookie", webHost.Cookies);
            }

            details.RequestHeaders.Add("Accept-Charset", "utf-8");

            if (postData != null) {
                var requestBodyText = postData
                    .SelectMany(kv => kv.Value.Select(v => new { k = kv.Key, v }))
                    .Select((kv, n) => new { p = HttpUtility.UrlEncode(kv.k) + "=" + HttpUtility.UrlEncode(kv.v), n })
                    .Aggregate("", (a, x) => a + (x.n == 0 ? "" : "&") + x.p);

                if (requestMethod == "POST")
                    details.PostData = Encoding.Default.GetBytes(requestBodyText);
                else
                    details.Query = requestBodyText;
            }

            webHost.Execute(() => {
                var output = new StringWriter();
                var worker = new Worker(details, output);
                HttpRuntime.ProcessRequest(worker);
                details.ResponseText = output.ToString();
            });

            string setCookie;
            if (details.ResponseHeaders.TryGetValue("Set-Cookie", out setCookie)) {
                // Trace.WriteLine(string.Format("Set-Cookie: {0}", setCookie));
                var cookieName = setCookie.Split(';')[0].Split('=')[0];
                DateTime expires;
                if (!string.IsNullOrEmpty(webHost.Cookies)
                    && setCookie.Contains("expires=")
                    && DateTime.TryParse(setCookie.Split(new[] { "expires=" }, 2, StringSplitOptions.None)[1].Split(';')[0], out expires)
                    && expires < DateTime.Now) {
                    // remove
                    // Trace.WriteLine(string.Format("Removing cookie: {0}", cookieName));
                    webHost.Cookies = Regex.Replace(webHost.Cookies, string.Format("{0}=[^;]*;?", cookieName), "");
                }
                else if (!string.IsNullOrEmpty(webHost.Cookies)
                    && Regex.IsMatch(webHost.Cookies, string.Format("\b{0}=", cookieName))) {
                    // replace
                    // Trace.WriteLine(string.Format("Replacing cookie: {0}", cookieName));
                    webHost.Cookies = Regex.Replace(webHost.Cookies, string.Format("{0}=[^;]*(;?)", cookieName), string.Format("{0}$1", setCookie.Split(';')[0]));
                }
                else {
                    // add
                    // Trace.WriteLine(string.Format("Adding cookie: {0}", cookieName));
                    webHost.Cookies = (webHost.Cookies + ';' + setCookie.Split(';').FirstOrDefault()).Trim(';');
                }
                // Trace.WriteLine(string.Format("Cookie jar: {0}", webHost.Cookies));
            }

            return details;
        }

        private static string StripVDir(string urlPath, string virtualDirectory) {
            if (urlPath == "/")
                return urlPath;

            return urlPath.StartsWith(virtualDirectory, StringComparison.OrdinalIgnoreCase)
                ? urlPath.Substring(virtualDirectory.Length)
                : urlPath;
        }

        public static RequestDetails SendRequest(this WebHost webHost, string urlPath) {
            return webHost.SendRequest(urlPath, null);
        }

        class Worker : SimpleWorkerRequest {
            private readonly RequestDetails _details;
            private readonly TextWriter _output;

            public Worker(RequestDetails details, TextWriter output)
                : base(details.Page, details.Query, output) {
                _details = details;
                _output = output;
                PostContentType = "application/x-www-form-urlencoded; charset=utf-8";
            }

            public string PostContentType { get; set; }


            public override String GetHttpVerbName() {
                if (_details.PostData == null)
                    return base.GetHttpVerbName();

                return "POST";
            }

            public override string GetKnownRequestHeader(int index) {
                if (index == HeaderContentLength) {
                    if (_details.PostData != null)
                        return _details.PostData.Length.ToString();
                }
                else if (index == HeaderContentType) {
                    if (_details.PostData != null)
                        return PostContentType;
                }
                else if (index == HeaderCookie) {
                    string value;
                    if (_details.RequestHeaders.TryGetValue("Cookie", out value))
                        return value;
                }
                else if (index == HeaderHost) {
                    return _details.HostName;
                }
                return base.GetKnownRequestHeader(index);

            }

            public override byte[] GetPreloadedEntityBody() {
                if (_details.PostData != null)
                    return _details.PostData;

                return base.GetPreloadedEntityBody();
            }

            public override void SendStatus(int statusCode, string statusDescription) {
                _details.StatusCode = statusCode;
                _details.StatusDescription = statusDescription;

                base.SendStatus(statusCode, statusDescription);
            }

            public override void SendKnownResponseHeader(int index, string value) {
                switch (index) {
                    case HeaderSetCookie:
                        _details.ResponseHeaders.Add("Set-Cookie", value);
                        break;
                    case HeaderLocation:
                        _details.ResponseHeaders.Add("Location", value);
                        break;
                    case HeaderContentType:
                        _details.ResponseHeaders.Add("Content-Type", value);
                        break;
                    default:
                        _details.ResponseHeaders.Add("known header #" + index, value);
                        break;
                }
                base.SendKnownResponseHeader(index, value);
            }

            public override void SendUnknownResponseHeader(string name, string value) {
                _details.ResponseHeaders.Add(name, value);

                base.SendUnknownResponseHeader(name, value);
            }

            public override void SendResponseFromFile(string filename, long offset, long length) {
                _output.Write(System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(filename)));
            }

            public override void SendResponseFromMemory(byte[] data, int length) {
                _output.Write(System.Text.Encoding.UTF8.GetString(data));
            }
        }
    }
}


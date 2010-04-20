using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using HtmlAgilityPack;
using Orchard.Specs.Util;

namespace Orchard.Specs.Hosting {
    public static class RequestExtensions {
        public static RequestDetails SendRequest(this WebHost webHost, string urlPath, IDictionary<string, IEnumerable<string>> postData) {
            var physicalPath = Bleroy.FluentPath.Path.Get(webHost.PhysicalDirectory);

            var details = new RequestDetails {
                UrlPath = urlPath,
                Page = physicalPath
                    .Combine(urlPath.TrimStart('/', '\\'))
                    .GetRelativePath(physicalPath),
            };

            if (postData != null) {
                var requestBodyText = postData
                    .SelectMany(kv=>kv.Value.Select(v=>new{k=kv.Key, v}))
                    .Select((kv, n) => new {p = HttpUtility.UrlEncode(kv.k) + "=" + HttpUtility.UrlEncode(kv.v), n})
                    .Aggregate("", (a, x) => a + (x.n == 0 ? "" : "&") + x.p);
                details.PostData = Encoding.Default.GetBytes(requestBodyText);
            }

            webHost.Execute(() => {
                var output = new StringWriter();
                HttpRuntime.ProcessRequest(new Worker(details, output));
                details.ResponseText = output.ToString();
            });

            return details;
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
                PostContentType = "application/x-www-form-urlencoded";
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

            public override void SendResponseFromFile(string filename, long offset, long length) {
                _output.Write(File.ReadAllText(filename));
            }
        }
    }
}


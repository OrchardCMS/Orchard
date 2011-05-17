using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace Orchard.WarmupStarter {
    public static class WarmupUtility {
        public static readonly string WarmupFilesPath = "~/App_Data/Warmup/";
        /// <summary>
        /// return true to put request on hold (until call to Signal()) - return false to allow pipeline to execute immediately
        /// </summary>
        /// <param name="httpApplication"></param>
        /// <returns></returns>
        public static bool DoBeginRequest(HttpApplication httpApplication) {
            // use the url as it was requested by the client
            // the real url might be different if it has been translated (proxy, load balancing, ...)
            var url = ToUrlString(httpApplication.Request);
            var virtualFileCopy = WarmupUtility.EncodeUrl(url.Trim('/'));
            var localCopy = Path.Combine(HostingEnvironment.MapPath(WarmupFilesPath), virtualFileCopy);

            if (File.Exists(localCopy)) {
                // result should not be cached, even on proxies
                httpApplication.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                httpApplication.Response.Cache.SetValidUntilExpires(false);
                httpApplication.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                httpApplication.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                httpApplication.Response.Cache.SetNoStore();

                httpApplication.Response.WriteFile(localCopy);
                httpApplication.Response.End();
                return true;
            }

            // there is no local copy and the file exists
            // serve the static file
            if (File.Exists(httpApplication.Request.PhysicalPath)) {
                return true;
            }

            return false;
        }

        public static string ToUrlString(HttpRequest request) {
            return string.Format("{0}://{1}{2}", request.Url.Scheme, request.Headers["Host"], request.RawUrl);
        }

        public static string EncodeUrl(string url) {
            if (String.IsNullOrWhiteSpace(url)) {
                throw new ArgumentException("url can't be empty");
            }

            var sb = new StringBuilder();
            foreach (var c in url.ToLowerInvariant()) {
                // only accept alphanumeric chars
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) {
                    sb.Append(c);
                }
                    // otherwise encode them in UTF8
                else {
                    sb.Append("_");
                    foreach (var b in Encoding.UTF8.GetBytes(new[] { c })) {
                        sb.Append(b.ToString("X"));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
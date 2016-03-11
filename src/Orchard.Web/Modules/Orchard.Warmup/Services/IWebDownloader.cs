
using System.Net;

namespace Orchard.Warmup.Services {
    public class DownloadResult {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
    }

    public interface IWebDownloader : IDependency {
        DownloadResult Download(string url);
    }
}

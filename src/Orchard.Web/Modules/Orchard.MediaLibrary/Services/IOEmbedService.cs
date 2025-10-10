using System.Xml.Linq;

namespace Orchard.MediaLibrary.Services {
    public interface IOEmbedService : IDependency {
        XDocument DownloadMediaData(string url);
    }
}
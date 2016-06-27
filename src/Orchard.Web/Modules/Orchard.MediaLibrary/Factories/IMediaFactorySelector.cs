using System.IO;

namespace Orchard.MediaLibrary.Factories {
    public interface IMediaFactorySelector : IDependency {
        MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType, string contentType);
    }
}
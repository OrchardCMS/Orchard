using System.IO;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    public interface IMediaFactory {
        MediaPart CreateMedia(Stream stream, string path, string mimeType);
    }

    public class NullMediaFactory : IMediaFactory {
        public static IMediaFactory Instance {
            get { return new NullMediaFactory(); }
        }

        public MediaPart CreateMedia(Stream stream, string path, string mimeType) {
            return null;
        }
    }
}
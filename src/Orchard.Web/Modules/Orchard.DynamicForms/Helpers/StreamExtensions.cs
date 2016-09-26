using System.IO;

namespace Orchard.DynamicForms.Helpers {
    public static class StreamExtensions {
        public static T Reset<T>(this T stream) where T:Stream {
            stream.Position = 0;
            return stream;
        }
    }
}
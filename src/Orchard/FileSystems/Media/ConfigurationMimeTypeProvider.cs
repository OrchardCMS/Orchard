using System.Web;

namespace Orchard.FileSystems.Media {
    /// <summary>
    /// Returns the mime-type of the specified file path.
    /// </summary>
    public class ConfigurationMimeTypeProvider : IMimeTypeProvider {
        
        /// <summary>
        /// Returns the mime-type of the specified file path.
        /// </summary>
        public string GetMimeType(string path) {
            // Returns "application/octet-stream" for unmapped / unkown file types.
            return MimeMapping.GetMimeMapping(path);
        }
    }
}
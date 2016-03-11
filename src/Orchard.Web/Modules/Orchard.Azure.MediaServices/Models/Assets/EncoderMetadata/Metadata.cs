using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Orchard.FileSystems.Media;
using Orchard.Azure.MediaServices.Helpers;
using Newtonsoft.Json;

namespace Orchard.Azure.MediaServices.Models.Assets.EncoderMetadata {
    public class Metadata {

        public static Metadata Parse(string encoderMetadataXml, string privateLocatorUrl, string publicLocatorUrl, IMimeTypeProvider mimeTypeProvider) {
            var xml = XDocument.Parse(encoderMetadataXml);
            return new Metadata(xml, privateLocatorUrl, publicLocatorUrl, mimeTypeProvider);
        }

        private readonly XmlNamespaceManager _nsm;
        private readonly XDocument _xml;
        private readonly string _privateLocatorUrl;
        private readonly string _publicLocatorUrl;
        private readonly IMimeTypeProvider _mimeTypeProvider;
        private IEnumerable<AssetFile> _assetFiles;

        public Metadata(XDocument xml, string privateLocatorUrl, string publicLocatorUrl, IMimeTypeProvider mimeTypeProvider) {
            _nsm = NamespaceHelper.CreateNamespaceManager(xml);
            _xml = xml;
            _privateLocatorUrl = privateLocatorUrl;
            _publicLocatorUrl = publicLocatorUrl;
            _mimeTypeProvider = mimeTypeProvider;
        }

        /// <summary>
        /// A collection of media files contained in this asset.
        /// </summary>
        public IEnumerable<AssetFile> AssetFiles {
            get {
                if (_assetFiles == null) {
                    var assetFilesQuery =
                        from e in _xml.Root.XPathSelectElements("./me:AssetFile", _nsm)
                        select new AssetFile(e, this, _mimeTypeProvider);
                    _assetFiles = assetFilesQuery.ToArray();
                }
                return _assetFiles;
            }
        }

        [JsonIgnore]
        public string PrivateLocatorUrl {
            get {
                return _privateLocatorUrl;
            }
        }

        public string PublicLocatorUrl {
            get {
                return _publicLocatorUrl;
            }
        }
    }
}
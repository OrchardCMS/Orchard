using System.Xml;
using System.Xml.Linq;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.Azure.MediaServices.Services.Assets {
    public class AssetStorageProvider : IAssetStorageProvider {
        public void BindStorage(Asset asset) {
            var infoset = asset.Record.Infoset;
            asset.Storage = new SimpleFieldStorage(
                (name, valueType) => Get(infoset.Element, name),
                (name, valueType, value) => Set(infoset.Element, name, value));
        }

        private static string Get(XElement element, string name) {
            if (string.IsNullOrEmpty(name)) {
                return element.Value;
            }
            var valueAttribute = element.Attribute(XmlConvert.EncodeLocalName(name));
            return valueAttribute == null ? null : valueAttribute.Value;
        }

        private static void Set(XElement element, string name, string value) {
            if (string.IsNullOrEmpty(name)) {
                element.Value = value;
            }
            else {
                element.SetAttributeValue(XmlConvert.EncodeLocalName(name), value);
            }
        }
    }
}
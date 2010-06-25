using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData.Services {
    public class SettingsFormatter :
        IMapper<XElement, SettingsDictionary>,
        IMapper<SettingsDictionary, XElement> {

        public SettingsDictionary Map(XElement source) {
            if (source == null)
                return new SettingsDictionary();

            return new SettingsDictionary(source.Attributes().ToDictionary(attr => XmlConvert.DecodeName(attr.Name.LocalName), attr => attr.Value));
        }

        public XElement Map(SettingsDictionary source) {
            if (source == null)
                return new XElement("settings");

            return new XElement("settings", source.Where(kv => kv.Value != null).Select(kv => new XAttribute(XmlConvert.EncodeLocalName(kv.Key), kv.Value)));
        }
    }
}

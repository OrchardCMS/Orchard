using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Orchard.ContentManagement.MetaData.Services {
    public class SettingsFormatter :
        IMapper<XElement, IDictionary<string, string>>,
        IMapper<IDictionary<string, string>, XElement> {

        public IDictionary<string, string> Map(XElement source) {
            if (source == null)
                return new Dictionary<string, string>();

            return source.Attributes().ToDictionary(attr => XmlConvert.DecodeName(attr.Name.LocalName), attr => attr.Value);
        }

        public XElement Map(IDictionary<string, string> source) {
            if (source == null)
                return new XElement("settings");

            return new XElement("settings", source.Select(kv => new XAttribute(XmlConvert.EncodeLocalName(kv.Key), kv.Value)));
        }
    }
}

using System.Xml;
using System.Xml.Linq;

namespace Orchard.ContentManagement.FieldStorage.InfosetStorage {
    public class InfosetPart : ContentPart {
        public InfosetPart() {
            Infoset = new Infoset();
            VersionInfoset = new Infoset();
        }

        public Infoset Infoset { get; set; }
        public Infoset VersionInfoset { get; set; }

        public string Get(string partName, string fieldName, string valueName) {
            var partElement = Infoset.Element.Element(partName);
            if (partElement == null) {
                return null;
            }
            var fieldElement = partElement.Element(fieldName);
            if (fieldElement == null) {
                return null;
            }
            if (string.IsNullOrEmpty(valueName)) {
                return fieldElement.Value;
            }
            var valueAttribute = fieldElement.Attribute(XmlConvert.EncodeLocalName(valueName));
            if (valueAttribute == null) {
                return null;
            }
            return valueAttribute.Value;
        }

        public void Set(string partName, string fieldName, string valueName, string value) {
            var partElement = Infoset.Element.Element(partName);
            if (partElement == null) {
                partElement = new XElement(partName);
                Infoset.Element.Add(partElement);
            }
            var fieldElement = partElement.Element(fieldName);
            if (fieldElement == null) {
                fieldElement = new XElement(fieldName);
                partElement.Add(fieldElement);
            }
            if (string.IsNullOrEmpty(valueName)) {
                fieldElement.Value = value;
            }
            else {
                fieldElement.SetAttributeValue(XmlConvert.EncodeLocalName(valueName), value);
            }
        }
    }
}
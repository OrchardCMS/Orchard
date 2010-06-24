using System;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.FieldStorage.InfosetStorage {
    public class InfosetStorageProvider : IFieldStorageProvider {
        public string ProviderName {
            get { return FieldStorageProviderSelector.DefaultProviderName; }
        }

        public IFieldStorage BindStorage(ContentPart contentPart, ContentPartDefinition.Field partFieldDefinition) {
            var partName = XmlConvert.EncodeLocalName(contentPart.PartDefinition.Name);
            var fieldName = XmlConvert.EncodeLocalName(partFieldDefinition.Name);
            var infosetPart = contentPart.As<InfosetPart>();

            return new Storage {
                Getter = name => Get(infosetPart.Infoset.Element, partName, fieldName, name),
                Setter = (name, value) => Set(infosetPart.Infoset.Element, partName, fieldName, name, value)
            };
        }

        private static string Get(XElement element, string partName, string fieldName, string valueName) {
            var partElement = element.Element(partName);
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

        private static void Set(XElement element, string partName, string fieldName, string valueName, string value) {
            var partElement = element.Element(partName);
            if (partElement == null) {
                partElement = new XElement(partName);
                element.Add(partElement);
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

        class Storage : IFieldStorage {
            public Func<string, string> Getter { get; set; }
            public Action<string, string> Setter { get; set; }
        }
    }
}
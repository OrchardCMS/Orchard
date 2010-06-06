using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData.Services {
    public class ContentDefinitionWriter : IContentDefinitionWriter {
        private readonly IMapper<IDictionary<string, string>, XElement> _settingsWriter;

        public ContentDefinitionWriter(IMapper<IDictionary<string, string>, XElement> settingsWriter) {
            _settingsWriter = settingsWriter;
        }

        public XElement Export(ContentTypeDefinition typeDefinition) {
            var typeElement = NewElement(typeDefinition.Name, typeDefinition.Settings);
            
            foreach(var typePart in typeDefinition.Parts) {
                typeElement.Add(NewElement(typePart.PartDefinition.Name, typePart.Settings));
            }
            return typeElement;
        }

        public XElement Export(ContentPartDefinition partDefinition) {
            var partElement = NewElement(partDefinition.Name, partDefinition.Settings);
            foreach(var partField in partDefinition.Fields) {
                var partFieldElement = NewElement(partField.Name, partField.Settings);
                partFieldElement.SetAttributeValue("FieldType", partField.FieldDefinition.Name);
                partElement.Add(partFieldElement);
            }
            return partElement;
        }

        private XElement NewElement(string name, IDictionary<string, string> settings) {
            var element = new XElement(XmlConvert.EncodeLocalName(name));
            foreach(var settingAttribute in _settingsWriter.Map(settings).Attributes()) {
                element.Add(settingAttribute);
            }
            return element;
        }
    }
}

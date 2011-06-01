using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData.Services {
    public class ContentDefinitionWriter : IContentDefinitionWriter {
        private readonly IMapper<SettingsDictionary, XElement> _settingsWriter;

        public ContentDefinitionWriter(IMapper<SettingsDictionary, XElement> settingsWriter) {
            _settingsWriter = settingsWriter;
        }

        public XElement Export(ContentTypeDefinition typeDefinition) {
            var typeElement = NewElement(typeDefinition.Name, typeDefinition.Settings);
            if (typeElement.Attribute("DisplayName") == null && typeDefinition.DisplayName != null) {
                typeElement.Add(new XAttribute("DisplayName", typeDefinition.DisplayName));
            }

            foreach(var typePart in typeDefinition.Parts) {
                typeElement.Add(NewElement(typePart.PartDefinition.Name, typePart.Settings));
            }
            return typeElement;
        }

        public XElement Export(ContentPartDefinition partDefinition) {
            var partElement = NewElement(partDefinition.Name, partDefinition.Settings);
            foreach(var partField in partDefinition.Fields) {
                var attributeName = partField.Name + "." + partField.FieldDefinition.Name;
                var partFieldElement = NewElement(attributeName, partField.Settings);
                partElement.Add(partFieldElement);
            }
            return partElement;
        }

        private XElement NewElement(string name, SettingsDictionary settings) {
            var element = new XElement(XmlConvert.EncodeLocalName(name));
            foreach(var settingAttribute in _settingsWriter.Map(settings).Attributes()) {
                element.Add(settingAttribute);
            }
            return element;
        }
    }
}

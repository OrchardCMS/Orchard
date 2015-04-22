using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Services {
    public class ElementSerializer : IElementSerializer {
        private readonly IElementManager _elementManager;
        private readonly IElementFactory _elementFactory;

        public ElementSerializer(IElementManager elementManager, IElementFactory elementFactory) {
            _elementManager = elementManager;
            _elementFactory = elementFactory;
        }

        public Element Deserialize(string data, DescribeElementsContext describeContext) {
            if (String.IsNullOrWhiteSpace(data))
                return null;

            var token = JToken.Parse(data);
            var element = ParseNode(node: token, parent: null, index: 0, describeContext: describeContext);

            return element;
        }

        public string Serialize(Element element) {
            var dto = ToDto(element);
            return JToken.FromObject(dto).ToString(Formatting.None);
        }

        public object ToDto(Element element, int index = 0) {
            var container = element as Container;
            var dto = new {
                typeName = element.Descriptor.TypeName,
                data = element.Data.Serialize(),
                exportableData = element.ExportableData.Serialize(),
                index = index,
                elements = container != null ? container.Elements.Select(ToDto).ToList() : new List<object>(),
                isTemplated = element.IsTemplated,
                htmlId = element.HtmlId,
                htmlClass = element.HtmlClass,
                htmlStyle = element.HtmlStyle,
                rule = element.Rule
            };
            return dto;
        }

        public Element ParseNode(JToken node, Container parent, int index, DescribeElementsContext describeContext) {
            var elementTypeName = (string)node["typeName"];

            if (String.IsNullOrWhiteSpace(elementTypeName))
                return null;

            var data = (string)node["data"];
            var htmlId = (string)node["htmlId"];
            var htmlClass = (string)node["htmlClass"];
            var htmlStyle = (string)node["htmlStyle"];
            var rule = (string)node["rule"];
            var elementData = ElementDataHelper.Deserialize(data);
            var exportableData = ElementDataHelper.Deserialize((string)node["exportableData"]);
            var childNodes = node["elements"];
            var elementDescriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, elementTypeName);

            if (elementDescriptor == null)
                return null; // This happens if an element exists in a layout, but its type is no longer available due to its feature being disabled.

            var element = _elementFactory.Activate(elementDescriptor, e => {
                e.Container = parent;
                e.Index = index;
                e.Data = elementData;
                e.ExportableData = exportableData;
                e.HtmlId = htmlId;
                e.HtmlClass = htmlClass;
                e.HtmlStyle = htmlStyle;
                e.Rule = rule;
            });

            var container = element as Container;

            if (container != null)
                container.Elements = childNodes != null
                    ? childNodes.Select((x, i) => ParseNode(x, container, i, describeContext)).Where(x => x != null).ToList()
                    : new List<Element>();

            element.IsTemplated = node.Value<bool>("isTemplated");

            return element;
        }
    }
}
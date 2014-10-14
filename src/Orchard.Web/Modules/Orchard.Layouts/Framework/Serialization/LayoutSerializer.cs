using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Framework.Serialization {
    public class LayoutSerializer : ILayoutSerializer {
        private readonly IElementManager _elementManager;
        private readonly IElementFactory _elementFactory;

        public LayoutSerializer(IElementManager elementManager, IElementFactory elementFactory) {
            _elementManager = elementManager;
            _elementFactory = elementFactory;
        }

        public IEnumerable<IElement> Deserialize(string state, DescribeElementsContext describeContext) {
            var emptyList = Enumerable.Empty<IElement>();

            if (String.IsNullOrWhiteSpace(state))
                return emptyList;

            var token = JToken.Parse(state);
            var nodes = (JArray)token["elements"];
            var elements = nodes != null 
                ? nodes.Select((x, i) => ParseNode(node: x, parent: null, index: i, describeContext: describeContext)).Where(x => x != null).ToArray() 
                : emptyList;

            return elements;
        }

        public string Serialize(IEnumerable<IElement> elements) {
            var root = new {
                elements = elements.Select(Serialize).ToArray()
            };

            return JToken.FromObject(root).ToString();
        }

        private static object Serialize(IElement element, int index) {
            var container = element as IContainer;
            var dto = new {
                typeName = element.Descriptor.TypeName,
                state = element.State.Serialize(),
                index = index,
                elements = container != null ? container.Elements.Select(Serialize).ToList() : new List<object>(),
                isTemplated = element.IsTemplated
            };
            return dto;
        }

        private IElement ParseNode(JToken node, IContainer parent, int index, DescribeElementsContext describeContext) {
            var elementTypeName = (string)node["typeName"];

            if (String.IsNullOrWhiteSpace(elementTypeName))
                return null;

            var elementState = ElementStateHelper.Deserialize((string)node["state"]);
            var childNodes = node["elements"];
            var elementDescriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, elementTypeName);

            if (elementDescriptor == null)
                return null; // This happens if an element exists in a layout, but its type is no longer available due to its feature being disabled.

            var element = _elementFactory.Activate(elementDescriptor, 
                new ActivateElementArgs {
                    Container = parent, 
                    Index = index, 
                    ElementState = elementState
                });
            var container = element as IContainer;

            if (container != null)
                container.Elements = childNodes != null 
                    ? childNodes.Select((x, i) => ParseNode(x, container, i, describeContext)).Where(x => x != null).ToList() 
                    : new List<IElement>();

            element.IsTemplated = node.Value<bool>("isTemplated");

            return element;
        }
    }
}
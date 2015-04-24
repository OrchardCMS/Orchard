using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    public class LayoutModelMapper : ILayoutModelMapper {
        private readonly ILayoutSerializer _serializer;
        private readonly IElementManager _elementManager;
        private readonly Lazy<IEnumerable<ILayoutModelMap>> _maps;

        public LayoutModelMapper(ILayoutSerializer serializer, IElementManager elementManager, Lazy<IEnumerable<ILayoutModelMap>> maps) {
            _serializer = serializer;
            _elementManager = elementManager;
            _maps = maps;
        }

        public object ToEditorModel(string layoutData, DescribeElementsContext describeContext) {
            var elements = _serializer.Deserialize(layoutData, describeContext);
            var canvas = elements.FirstOrDefault(x => x is Canvas) ?? new Canvas();
            return ToEditorModel(canvas, describeContext);
        }

        public object ToEditorModel(Element element, DescribeElementsContext describeContext) {
            var map = GetMapFor(element);
            var node = new JObject();
            var container = element as Container;

            // We want to convert any null value being added to this node to an empty string,
            // so that we can perform a JSON string comparison on the client side editor to detect if the user changed anything.
            // If the initial state would contain null values, these would become empty strings after the user made a change
            // (e.g. setting some HtmlID property from empty to "my-id" and then clearing out that field).
            node.PropertyChanged += (sender, args) => {
                var value = node[args.PropertyName] as JValue;

                if (value != null && value.Value == null)
                    node[args.PropertyName] = "";
            };

            map.FromElement(element, describeContext, node);
            node["type"] = map.LayoutElementType;

            if (container != null)
                node["children"] = new JArray(container.Elements.Select(x => ToEditorModel(x, describeContext)).ToList());

            // Would be nicer if we could turn JObject into an anonymous object directly, but this seems to be the only way.
            return JsonConvert.DeserializeObject(node.ToString());
        }

        public IEnumerable<Element> ToLayoutModel(string editorData, DescribeElementsContext describeContext) {
            if (String.IsNullOrWhiteSpace(editorData))
                yield break;

            var canvas = JToken.Parse(editorData);
            yield return ParseEditorNode(node: canvas, parent: null, index: 0, describeContext: describeContext);
        }

        public ILayoutModelMap GetMapFor(Element element) {
            return SelectMap(x => x.CanMap(element));
        }

        private Element ParseEditorNode(JToken node, Container parent, int index, DescribeElementsContext describeContext) {
            var element = LoadElement(node, parent, index, describeContext);
            var childNodes = (JArray)node["children"];
            var container = element as Container;

            if (container != null)
                container.Elements = childNodes != null
                    ? childNodes.Select((x, i) => ParseEditorNode(x, container, i, describeContext)).Where(x => x != null).ToList()
                    : new List<Element>();

            return element;
        }

        private Element LoadElement(JToken node, Container parent, int index, DescribeElementsContext describeContext) {
            var type = (string)node["type"];
            var map = SelectMap(x => x.LayoutElementType == type);
            var element = map.ToElement(_elementManager, describeContext, node);

            element.Container = parent;
            element.Index = index;

            return element;
        }

        private ILayoutModelMap SelectMap(Func<ILayoutModelMap, bool> predicate) {
            return _maps.Value.OrderByDescending(x => x.Priority).FirstOrDefault(predicate);
        }
    }
}
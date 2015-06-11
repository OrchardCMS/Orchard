using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    public class LayoutSerializer : ILayoutSerializer {
        private readonly IElementSerializer _elementSerializer;
        public LayoutSerializer(IElementSerializer elementSerializer) {
            _elementSerializer = elementSerializer;
        }

        public IEnumerable<Element> Deserialize(string data, DescribeElementsContext describeContext) {
            var emptyList = Enumerable.Empty<Element>();

            if (String.IsNullOrWhiteSpace(data))
                return emptyList;

            var token = JToken.Parse(data);
            var nodes = (JArray)token["elements"];
            var elements = nodes != null 
                ? nodes.Select((x, i) => _elementSerializer.ParseNode(node: x, parent: null, index: i, describeContext: describeContext)).Where(x => x != null).ToArray() 
                : emptyList;

            return elements;
        }

        public string Serialize(IEnumerable<Element> elements) {
            var root = new {
                elements = elements.Select((x, i) => _elementSerializer.ToDto(x, i)).ToArray()
            };

            return JToken.FromObject(root).ToString(Formatting.None);
        }
    }
}
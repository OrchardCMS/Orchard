using Newtonsoft.Json.Linq;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    public interface IElementSerializer : IDependency {
        Element Deserialize(string data, DescribeElementsContext describeContext);
        string Serialize(Element element);
        object ToDto(Element element, int index = 0);
        Element ParseNode(JToken node, Container parent, int index, DescribeElementsContext describeContext);
    }
}
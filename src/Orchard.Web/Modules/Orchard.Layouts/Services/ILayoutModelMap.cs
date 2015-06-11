using Newtonsoft.Json.Linq;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    public interface ILayoutModelMap : IDependency {
        int Priority { get; }
        string LayoutElementType { get; }
        bool CanMap(Element element);
        Element ToElement(IElementManager elementManager, DescribeElementsContext describeContext, JToken node);
        void FromElement(Element element, DescribeElementsContext describeContext, JToken node);
    }
}
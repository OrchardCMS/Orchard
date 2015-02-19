using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    public interface ILayoutSerializer : IDependency {
        IEnumerable<Element> Deserialize(string data, DescribeElementsContext describeContext);
        string Serialize(IEnumerable<Element> elements);
    }
}
using System.Collections.Generic;
using Orchard.Services;

namespace Orchard.Layouts.Services {
    public interface IElementFilterProcessor : IDependency {
        string ProcessContent(string text, string flavor, IDictionary<string, object> context);
    }
}
using System.Collections.Generic;
using Orchard.Services;

namespace Orchard.Layouts.Services {
    public interface IElementFilter : IHtmlFilter {
        string ProcessContent(string text, string flavor, IDictionary<string, object> context);
    }
}
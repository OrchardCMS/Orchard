using System;

namespace Orchard.Services {
    [Obsolete("Implement IHtmlFilter2 or HtmlFilterBase instead.")]
    public interface IHtmlFilter : IDependency {
        string ProcessContent(string text, string flavor);
    }
}
using System;

namespace Orchard.Services {
    public abstract class HtmlFilter : Component, IHtmlFilter2
    {
        public abstract string ProcessContent(string text, HtmlFilterContext context);

        string IHtmlFilter.ProcessContent(string text, string flavor)
        {
            return ProcessContent(text, new HtmlFilterContext { Flavor = flavor });
        }
    }
}
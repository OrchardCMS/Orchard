namespace Orchard.Services {
    public abstract class HtmlFilter : Component, IHtmlFilter {
        public abstract string ProcessContent(string text, HtmlFilterContext context);
    }
}

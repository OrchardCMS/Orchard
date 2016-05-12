namespace Orchard.Services {
#pragma warning disable CS0618 // Type or member is obsolete
    public interface IHtmlFilter2 : IHtmlFilter
    {
#pragma warning restore CS0618 // Type or member is obsolete
        string ProcessContent(string text, HtmlFilterContext context);
    }
}
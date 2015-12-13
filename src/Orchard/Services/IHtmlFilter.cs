namespace Orchard.Services {
    public interface IHtmlFilter : IDependency {
        string ProcessContent(string text, string flavor);
    }
}
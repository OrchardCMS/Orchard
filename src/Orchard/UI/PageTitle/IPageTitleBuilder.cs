namespace Orchard.UI.PageTitle {
    public interface IPageTitleBuilder : IDependency {
        void AddTitleParts(params string[] titleParts);
        void AppendTitleParts(params string[] titleParts);
        string GenerateTitle();
    }
}
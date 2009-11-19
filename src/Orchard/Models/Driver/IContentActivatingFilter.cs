namespace Orchard.Models.Driver {
    public interface IContentActivatingFilter : IContentFilter {
        void Activating(ActivatingContentContext context);
    }
}

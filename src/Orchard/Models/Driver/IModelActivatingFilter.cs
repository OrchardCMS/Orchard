namespace Orchard.Models.Driver {
    public interface IModelActivatingFilter : IModelFilter {
        void Activating(ActivatingModelContext context);
    }
}

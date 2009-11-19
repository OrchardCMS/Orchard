namespace Orchard.Models.Driver {
    public interface IUpdateModel {
        bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class;
    }
}
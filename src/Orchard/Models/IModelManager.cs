namespace Orchard.Models {
    public interface IModelManager {
        IModel New(string modelType);
    }
}

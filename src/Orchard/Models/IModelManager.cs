namespace Orchard.Models {
    public interface IModelManager {
        IModel New(string modelType);
        IModel Get(int id);
    }
}

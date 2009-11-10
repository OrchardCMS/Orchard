namespace Orchard.Models {
    public interface IModelManager : IDependency {
        IModel New(string modelType);
        IModel Get(int id);
        void Create(IModel model);
    }
}

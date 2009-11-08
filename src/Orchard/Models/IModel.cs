namespace Orchard.Models {
    public interface IModel {
        int Id { get; }
        string ModelType { get; }

        bool Is<T>() where T : class, IModel;
        T As<T>() where T : class, IModel;

        void Weld(IModel model);
    }
}

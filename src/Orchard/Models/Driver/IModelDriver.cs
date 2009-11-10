namespace Orchard.Models.Driver {
    public interface IModelDriver : IDependency {
        void New(NewModelContext context);
        void Load(LoadModelContext context);
    }
}
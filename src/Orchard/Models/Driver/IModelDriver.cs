namespace Orchard.Models.Driver {
    public interface IModelDriver : IDependency {
        void New(NewModelContext context);
        void Create(CreateModelContext context);
        void Load(LoadModelContext context);

        void GetEditors(GetModelEditorsContext context);
        void UpdateEditors(UpdateModelContext context);
    }
}
namespace Orchard.Models.Driver {
    public interface IContentHandler : IDependency {
        void Activating(ActivatingContentContext context);
        void Activated(ActivatedContentContext context);
        void Creating(CreateContentContext context);
        void Created(CreateContentContext context);
        void Loading(LoadContentContext context);
        void Loaded(LoadContentContext context);

        void GetEditors(GetContentEditorsContext context);
        void UpdateEditors(UpdateContentContext context);
    }
}
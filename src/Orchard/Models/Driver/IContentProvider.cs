using System.Collections.Generic;

namespace Orchard.Models.Driver {
    public interface IContentProvider : IDependency {
        IEnumerable<ContentType> GetContentTypes();

        void Activating(ActivatingContentContext context);
        void Activated(ActivatedContentContext context);
        void Creating(CreateContentContext context);
        void Created(CreateContentContext context);
        void Loading(LoadContentContext context);
        void Loaded(LoadContentContext context);

        void GetDisplays(GetDisplaysContext context);
        void GetEditors(GetEditorsContext context);
        void UpdateEditors(UpdateContentContext context);
    }
}
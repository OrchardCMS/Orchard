using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public interface IContentItemDriver : IEvents {
        IEnumerable<ContentType> GetContentTypes();
        void GetContentItemMetadata(GetContentItemMetadataContext context);

        DriverResult BuildDisplayModel(BuildDisplayModelContext context);
        DriverResult BuildEditorModel(BuildEditorModelContext context);
        DriverResult UpdateEditorModel(UpdateEditorModelContext context);
    }
}
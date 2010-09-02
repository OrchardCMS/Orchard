using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers {
    public interface IContentFieldDriver : IEvents {
        DriverResult BuildDisplayShape(BuildDisplayModelContext context);
        DriverResult BuildEditorShape(BuildEditorModelContext context);
        DriverResult UpdateEditorShape(UpdateEditorModelContext context);

        IEnumerable<ContentFieldInfo> GetFieldInfo();
    }
}
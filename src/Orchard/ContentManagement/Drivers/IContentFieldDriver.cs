using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers {
    public interface IContentFieldDriver : IDependency {
        DriverResult BuildDisplayShape(BuildDisplayContext context);
        DriverResult BuildEditorShape(BuildEditorContext context);
        DriverResult UpdateEditorShape(UpdateEditorContext context);

        IEnumerable<ContentFieldInfo> GetFieldInfo();
    }
}
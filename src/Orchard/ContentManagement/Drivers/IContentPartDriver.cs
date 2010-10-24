using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers {
    public interface IContentPartDriver : IDependency {
        DriverResult BuildDisplay(BuildDisplayContext context);
        DriverResult BuildEditor(BuildEditorContext context);
        DriverResult UpdateEditor(UpdateEditorContext context);

        IEnumerable<ContentPartInfo> GetPartInfo();
    }
}
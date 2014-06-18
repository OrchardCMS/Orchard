using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers {
    public interface IContentPartDriver : IDependency {

        [Obsolete("Use BuildDisplayAsync")]
        DriverResult BuildDisplay(BuildDisplayContext context);
        [Obsolete("Use BuildEditorAsync")]
        DriverResult BuildEditor(BuildEditorContext context);
        [Obsolete("Use UpdateEditorAsync")]
        DriverResult UpdateEditor(UpdateEditorContext context);

        Task<DriverResult> BuildDisplayAsync(BuildDisplayContext context);
        Task<DriverResult> BuildEditorAsync(BuildEditorContext context);
        Task<DriverResult> UpdateEditorAsync(UpdateEditorContext context);
        void Importing(ImportContentContext context);
        void Imported(ImportContentContext context);
        void Exporting(ExportContentContext context);
        void Exported(ExportContentContext context);
        IEnumerable<ContentPartInfo> GetPartInfo();
        void GetContentItemMetadata(GetContentItemMetadataContext context);
    }
}
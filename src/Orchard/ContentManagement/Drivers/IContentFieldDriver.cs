using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers {
    public interface IContentFieldDriver : IDependency {
        Task<DriverResult> BuildDisplayShapeAsync(BuildDisplayContext context);
        Task<DriverResult> BuildEditorShapeAsync(BuildEditorContext context);
        Task<DriverResult> UpdateEditorShapeAsync(UpdateEditorContext context);
        void Importing(ImportContentContext context);
        void Imported(ImportContentContext context);
        void Exporting(ExportContentContext context);
        void Exported(ExportContentContext context);
        void Describe(DescribeMembersContext context);
        IEnumerable<ContentFieldInfo> GetFieldInfo();
        void GetContentItemMetadata(GetContentItemMetadataContext context);
    }
}
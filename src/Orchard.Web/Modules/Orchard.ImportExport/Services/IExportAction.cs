using Orchard.ContentManagement;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public interface IExportAction : IDependency {
        int Priority { get; }
        string Name { get; }
        
        dynamic BuildEditor(dynamic shapeFactory);
        dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater);
        void Configure(ExportActionConfigurationContext context);
        void ConfigureDefault();
        void Execute(ExportActionContext context);
    }
}
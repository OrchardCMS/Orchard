using Orchard.ContentManagement;

namespace Orchard.ImportExport.Services {
    public interface IExportAction : IDependency {
        int Priority { get; }
        string Name { get; }
        
        dynamic BuildEditor(dynamic shapeFactory);
        dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater);
        void Execute(ExportActionContext exportActionContext);
    }
}
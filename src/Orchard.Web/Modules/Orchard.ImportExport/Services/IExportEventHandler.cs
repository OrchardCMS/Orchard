using Orchard.Events;

namespace Orchard.ImportExport.Services {
    public interface IExportEventHandler : IEventHandler {
        void Exporting(ExportContext context);
        void Exported(ExportContext context);
    }
}
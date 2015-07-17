using System;
using Orchard.Events;

namespace Orchard.ImportExport.Services {
    [Obsolete("Implement IRecipeExecutionStep instead.")]
    public interface IExportEventHandler : IEventHandler {
        void Exporting(ExportContext context);
        void Exported(ExportContext context);
    }
}
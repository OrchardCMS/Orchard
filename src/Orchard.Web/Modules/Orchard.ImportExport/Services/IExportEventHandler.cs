using System;
using Orchard.Events;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    [Obsolete("Implement IRecipeExecutionStep instead.")]
    public interface IExportEventHandler : IEventHandler {
        void Exporting(ExportContext context);
        void Exported(ExportContext context);
    }
}
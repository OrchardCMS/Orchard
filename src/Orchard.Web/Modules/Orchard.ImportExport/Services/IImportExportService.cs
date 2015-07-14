using System.Collections.Generic;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        string Import(string recipeText);
        string Export(IEnumerable<IExportStepProvider> steps, ExportOptions exportOptions);
    }
}




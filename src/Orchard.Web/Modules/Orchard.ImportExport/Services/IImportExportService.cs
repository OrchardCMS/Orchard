using System.Collections.Generic;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        void Import(string recipeText);
        string Export(IEnumerable<string> contentTypes, ExportOptions exportOptions);
    }
}




using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        string Import(string recipeText);
        string Export(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems, ExportOptions exportOptions);
        string Export(IEnumerable<string> contentTypes, ExportOptions exportOptions);
    }
}




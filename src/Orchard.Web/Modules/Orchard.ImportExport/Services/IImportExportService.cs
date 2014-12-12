using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        string Import(string recipeText);
        FilePathResult Export(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems, ExportOptions exportOptions);
        FilePathResult Export(IEnumerable<string> contentTypes, ExportOptions exportOptions);
    }
}

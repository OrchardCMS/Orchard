using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        string Import(Stream recipeOrPackage, string fileName);
        string ImportRecipe(string recipeText, string filesPath = null, string executionId = null);
        FilePathResult Export(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems, ExportOptions exportOptions);
        FilePathResult Export(IEnumerable<string> contentTypes, ExportOptions exportOptions);
    }
}

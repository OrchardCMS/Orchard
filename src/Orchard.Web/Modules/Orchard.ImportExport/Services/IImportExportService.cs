using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.Recipes.Services;

namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        string Import(XDocument recipeDocument);
        XDocument Export(IEnumerable<IRecipeBuilderStep> steps);
        string WriteExportFile(XDocument recipeDocument);
    }

    public static class ImportExportServiceExtensions {
        public static string Import(this IImportExportService service, string recipeText) {
            var recipeDocument = XDocument.Parse(recipeText, LoadOptions.PreserveWhitespace);
            return service.Import(recipeDocument);
        }
    }
}
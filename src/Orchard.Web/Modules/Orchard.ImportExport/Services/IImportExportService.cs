using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.Recipes.Services;

namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        string Import(string recipeText);
        XDocument ExportXml(IEnumerable<IRecipeBuilderStep> steps);
        string Export(IEnumerable<IRecipeBuilderStep> steps);
        string WriteExportFile(XDocument recipeDocument);
        string WriteExportFile(string recipeText);
    }
}
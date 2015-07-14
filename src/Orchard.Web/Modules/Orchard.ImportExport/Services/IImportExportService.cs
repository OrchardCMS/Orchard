using System.Collections.Generic;
using Orchard.Recipes.Services;

namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        string Import(string recipeText);
        string Export(IEnumerable<IRecipeBuilderStep> steps);
    }
}
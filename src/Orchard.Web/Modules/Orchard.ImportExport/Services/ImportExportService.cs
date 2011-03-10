using JetBrains.Annotations;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Services;

namespace Orchard.ImportExport.Services {
    [UsedImplicitly]
    public class ImportExportService : IImportExportService {
        public ImportExportService(IRecipeParser recipeParser, IRecipeManager recipeManager) {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void Import(string recipe) {
        }
    }
}
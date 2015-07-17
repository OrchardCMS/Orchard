using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.FileSystems.AppData;
using Orchard.Recipes.Services;
using Orchard.Services;

namespace Orchard.ImportExport.Services {
    public class ImportExportService : Component, IImportExportService {
        private readonly IOrchardServices _orchardServices;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IRecipeBuilder _recipeBuilder;
        private readonly IRecipeParser _recipeParser;
        private readonly IRecipeExecutor _recipeExecutor;
        private readonly IClock _clock;
        private const string ExportsDirectory = "Exports";

        public ImportExportService(
            IOrchardServices orchardServices,
            IAppDataFolder appDataFolder,
            IRecipeBuilder recipeBuilder,
            IRecipeParser recipeParser,
            IRecipeExecutor recipeExecutor,
            IClock clock) {

            _orchardServices = orchardServices;
            _appDataFolder = appDataFolder;
            _recipeBuilder = recipeBuilder;
            _recipeParser = recipeParser;
            _recipeExecutor = recipeExecutor;
            _clock = clock;
        }

        public string Import(XDocument recipeDocument) {
            var recipe = _recipeParser.ParseRecipe(recipeDocument);
            return _recipeExecutor.Execute(recipe);
        }

        public XDocument Export(IEnumerable<IRecipeBuilderStep> steps) {
            var recipe = _recipeBuilder.Build(steps);
            return recipe;
        }

        public string WriteExportFile(XDocument recipeDocument) {
            var exportFile = String.Format("Export-{0}-{1}.xml", _orchardServices.WorkContext.CurrentUser.UserName, _clock.UtcNow.Ticks);
            if (!_appDataFolder.DirectoryExists(ExportsDirectory)) {
                _appDataFolder.CreateDirectory(ExportsDirectory);
            }

            var path = _appDataFolder.Combine(ExportsDirectory, exportFile);
            var recipeText = recipeDocument.ToString(SaveOptions.None);
            _appDataFolder.CreateFile(path, recipeText);

            return _appDataFolder.MapPath(path);
        }
    }
}
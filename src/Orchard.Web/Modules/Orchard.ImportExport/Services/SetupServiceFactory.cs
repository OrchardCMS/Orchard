using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.State;
using Orchard.Recipes.Services;
using Orchard.Setup.Services;

namespace Orchard.ImportExport.Services {
    /// <summary>
    /// We need to manually instantiate the SetupService class because the Orchard.Setup feature will be disabled after setup completes.
    /// </summary>
    public class SetupServiceFactory : ISetupServiceFactory {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IProcessingEngine _processingEngine;
        private readonly IRecipeHarvester _recipeHarvester;

        public SetupServiceFactory(
            ShellSettings shellSettings,
            IOrchardHost orchardHost,
            IShellSettingsManager shellSettingsManager,
            IShellContainerFactory shellContainerFactory,
            ICompositionStrategy compositionStrategy,
            IProcessingEngine processingEngine,
            IRecipeHarvester recipeHarvester) {

            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;
            _shellContainerFactory = shellContainerFactory;
            _compositionStrategy = compositionStrategy;
            _processingEngine = processingEngine;
            _recipeHarvester = recipeHarvester;
        }

        public ISetupService CreateSetupService() {
            return new SetupService(
                _shellSettings,
                _orchardHost,
                _shellSettingsManager,
                _shellContainerFactory,
                _compositionStrategy,
                _processingEngine,
                _recipeHarvester);
        }
    }
}
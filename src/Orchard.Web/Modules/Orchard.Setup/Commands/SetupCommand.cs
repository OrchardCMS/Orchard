using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Commands;
using Orchard.Recipes.Services;
using Orchard.Setup.Services;

namespace Orchard.Setup.Commands {
    public class SetupCommand : DefaultOrchardCommandHandler {
        private readonly ISetupService _setupService;
        private readonly IRecipeHarvester _recipeHarvester;

        public SetupCommand(ISetupService setupService, IRecipeHarvester recipeHarvester) {
            _setupService = setupService;
            _recipeHarvester = recipeHarvester;
        }

        [OrchardSwitch]
        public string SiteName { get; set; }

        [OrchardSwitch]
        public string AdminUsername { get; set; }

        [OrchardSwitch]
        public string AdminPassword { get; set; }

        [OrchardSwitch]
        public string DatabaseProvider { get; set; }

        [OrchardSwitch]
        public string DatabaseConnectionString { get; set; }

        [OrchardSwitch]
        public string DatabaseTablePrefix { get; set; }

        [OrchardSwitch]
        public string EnabledFeatures { get; set; }

        [OrchardSwitch]
        public string Recipe { get; set; }

        [CommandHelp("setup /SiteName:<siteName> /AdminUsername:<username> /AdminPassword:<password> /DatabaseProvider:<SqlCe|SQLServer|MySql|PostgreSql> " + 
            "/DatabaseConnectionString:<connection_string> /DatabaseTablePrefix:<table_prefix> /EnabledFeatures:<feature1,feature2,...> " +
            "/Recipe:<recipe>" + 
            "\r\n\tRuns first time setup for the site or for a given tenant.")]
        [CommandName("setup")]
        [OrchardSwitches("SiteName,AdminUsername,AdminPassword,DatabaseProvider,DatabaseConnectionString,DatabaseTablePrefix,EnabledFeatures,Recipe")]
        public void Setup() {
            IEnumerable<string> enabledFeatures = null;
            if (!String.IsNullOrEmpty(EnabledFeatures)) {
                enabledFeatures = EnabledFeatures
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !String.IsNullOrEmpty(s));
            }
            Recipe = String.IsNullOrEmpty(Recipe) ? "Default" : Recipe;
            var recipe = _setupService.Recipes().GetRecipeByName(Recipe);

            var setupContext = new SetupContext {
                SiteName = SiteName,
                AdminUsername = AdminUsername,
                AdminPassword = AdminPassword,
                DatabaseProvider = DatabaseProvider,
                DatabaseConnectionString = DatabaseConnectionString,
                DatabaseTablePrefix = DatabaseTablePrefix,
                EnabledFeatures = enabledFeatures,
                Recipe = recipe,
            };

            var executionId = _setupService.Setup(setupContext);

            Context.Output.WriteLine(T("Setup of site '{0}' was started with recipe execution ID {1}. Use the 'recipes result' command to check the result of the execution.", setupContext.SiteName, executionId));
        }
    }
}

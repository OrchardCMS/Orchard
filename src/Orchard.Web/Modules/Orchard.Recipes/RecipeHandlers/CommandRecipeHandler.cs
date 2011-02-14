using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers  {
    public class CommandRecipeHandler : IRecipeHandler {
        public CommandRecipeHandler () {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        /* 
         <Command>
            command1
            command2
            command3
         </Command>
        */
        // run Orchard commands.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Command", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var commands = 
                recipeContext.RecipeStep.Step.Value
                .Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(commandEntry => commandEntry.Trim());

            // run commands.
            recipeContext.Executed = true;
        }
    }
}
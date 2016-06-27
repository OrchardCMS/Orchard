using System;
using System.Web;
using System.Web.Routing;
using System.Linq;
using Orchard.Alias;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Alias.Recipes.Executors {
    public class AliasStep : RecipeExecutionStep {
        private readonly IAliasService _aliasService;

        public AliasStep(
            IAliasService aliasService,
            RecipeExecutionLogger logger) : base(logger) {
            _aliasService = aliasService;
        }

        public override string Name {
            get { return "Aliases"; }
        }

        /*
        <Aliases>
        <Alias Path="Profile/Edit" Area="Custom.Profile">
        <RouteValues>
        <Add Key="area" Value="Custom.Profile" />
        <Add Key="controller" Value="Profile" />
        <Add Key="action" Value="Edit" />
        </RouteValues>
        </Alias>
        */
        public override void Execute(RecipeExecutionContext context) {

            foreach (var aliasElement in context.RecipeStep.Step.Elements()) {
                var aliasPath = aliasElement.Attribute("Path").Value;

                Logger.Information("Importing alias '{0}'.", aliasPath);

                try {
                    var rvd = new RouteValueDictionary();

                    var routeValuesElement = aliasElement.Descendants("RouteValues").FirstOrDefault();

                    if (routeValuesElement != null) {
                        foreach (var routeValue in routeValuesElement.Descendants("Add")) {
                            rvd.Add(routeValue.Attribute("Key").Value, routeValue.Attribute("Value").Value);
                        }
                    }

                    _aliasService.Set(aliasPath, rvd, "Custom", false);
                }

                catch (Exception ex) {
                    Logger.Error(ex, "Error while processing alias '{0}'.", aliasPath);
                    throw;
                }
            }
        }
    }
}
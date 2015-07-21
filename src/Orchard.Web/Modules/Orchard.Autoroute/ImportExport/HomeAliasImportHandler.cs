using System;
using System.Linq;
using System.Web.Routing;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Autoroute.ImportExport {
    public class HomeAliasImportHandler : Component, IRecipeHandler {
        private readonly IHomeAliasService _homeAliasService;
        private readonly IContentManager _contentManager;

        public HomeAliasImportHandler(IHomeAliasService homeAliasService, IContentManager contentManager) {
            _homeAliasService = homeAliasService;
            _contentManager = contentManager;
        }

        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "HomeAlias", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var root = recipeContext.RecipeStep.Step;
            var routeValueDictionary = root.Elements().ToDictionary(x => x.Name.LocalName.ToLower(), x => (object)x.Value);
            var homePageIdentifier = root.Attr("Identifier");
            var homePageIdentity = new ContentIdentity(homePageIdentifier);
            var homePage = !String.IsNullOrEmpty(homePageIdentifier) ? _contentManager.ResolveIdentity(homePageIdentity) : default(ContentItem);
            
            if(homePage != null)
                _homeAliasService.PublishHomeAlias(homePage);
            else
                _homeAliasService.PublishHomeAlias(new RouteValueDictionary(routeValueDictionary));

            recipeContext.Executed = true;
        }
    }
}

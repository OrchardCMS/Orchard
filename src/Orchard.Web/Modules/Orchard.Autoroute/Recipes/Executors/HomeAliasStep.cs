using System;
using System.Linq;
using System.Web.Routing;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Autoroute.Recipes.Executors {
    public class HomeAliasStep : RecipeExecutionStep {
        private readonly IContentManager _contentManager;
        private readonly IHomeAliasService _homeAliasService;

        public HomeAliasStep(RecipeExecutionLogger logger, IContentManager contentManager, IHomeAliasService homeAliasService) : base(logger) {
            _contentManager = contentManager;
            _homeAliasService = homeAliasService;
        }

        public override string Name { get { return "HomeAlias"; } }
        
        public override void Execute(RecipeExecutionContext context) {
            var root = context.RecipeStep.Step;
            var routeValueDictionary = root.Elements().ToDictionary(x => x.Name.LocalName.ToLower(), x => (object)x.Value);
            var homePageIdentifier = root.Attr("Id");
            var homePageIdentity = new ContentIdentity(homePageIdentifier);
            var homePage = !String.IsNullOrEmpty(homePageIdentifier) ? _contentManager.ResolveIdentity(homePageIdentity) : default(ContentItem);

            if (homePage != null)
                _homeAliasService.PublishHomeAlias(homePage);
            else
                _homeAliasService.PublishHomeAlias(new RouteValueDictionary(routeValueDictionary));
        }
    }
}

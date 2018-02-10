using System;
using System.Linq;
using System.Web.Routing;
using System.Xml.Linq;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Recipes.Services;

namespace Orchard.Autoroute.Recipes.Builders {
    public class HomeAliasStep : RecipeBuilderStep {
        private readonly IHomeAliasService _homeAliasService;
        private readonly IContentManager _contentManager;

        public HomeAliasStep(IHomeAliasService homeAliasService, IContentManager contentManager) {
            _homeAliasService = homeAliasService;
            _contentManager = contentManager;
        }

        public override string Name {
            get { return "HomeAlias"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Home Alias"); }
        }

        public override LocalizedString Description {
            get { return T("Exports home alias."); }
        }

        public override void Build(BuildContext context) {
            var homeAliasRoute = _homeAliasService.GetHomeRoute() ?? new RouteValueDictionary();
            var root = new XElement("HomeAlias");
            var homePage = _homeAliasService.GetHomePage(VersionOptions.Latest);

            // If the home alias points to a content item, store its identifier in addition to the routevalues,
            // so we can publish the home page alias during import where the ID primary key value of the home page might have changed,
            // so we can't rely on the route values in that case.
            if (homePage != null) {
                var homePageIdentifier = _contentManager.GetItemMetadata(homePage).Identity.ToString();
                root.Attr("Id", homePageIdentifier);
            }
            else {
                // The alias does not point to a content item, so export the route values instead.
                root.Add(homeAliasRoute.Select(x => new XElement(Capitalize(x.Key), x.Value)).ToArray());
            }

            context.RecipeDocument.Element("Orchard").Add(root);
        }

        private string Capitalize(string value) {
            if (String.IsNullOrEmpty(value))
                return value;

            return Char.ToUpper(value[0]) + value.Substring(1);
        }
    }
}
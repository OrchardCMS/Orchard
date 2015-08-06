using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using System.Xml.Linq;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.Events;

namespace Orchard.Autoroute.ImportExport {
    public interface IExportEventHandler : IEventHandler {
        void Exporting(dynamic context);
        void Exported(dynamic context);
    }

    public class HomeAliasHandler : IExportEventHandler {
        private readonly IHomeAliasService _homeAliasService;
        private readonly IContentManager _contentManager;

        public HomeAliasHandler(IHomeAliasService homeAliasService, IContentManager contentManager) {
            _homeAliasService = homeAliasService;
            _contentManager = contentManager;
        }

        public void Exporting(dynamic context) {
        }

        public void Exported(dynamic context) {

            if (!((IEnumerable<string>)context.ExportOptions.CustomSteps).Contains("HomeAlias")) {
                return;
            }

            var homeAliasRoute = _homeAliasService.GetHomeRoute() ?? new RouteValueDictionary();
            var root = new XElement("HomeAlias", homeAliasRoute.Select(x => new XElement(Capitalize(x.Key), x.Value)));
            var homePage = _homeAliasService.GetHomePage(VersionOptions.Latest);

            // If the home alias points to a content item, store its identifier in addition to the routevalues,
            // so we can publish the home page alias during import where the ID primary key value of the home page might have changed,
            // so we can't rely on the route values in that case.
            if (homePage != null) {
                var homePageIdentifier = _contentManager.GetItemMetadata(homePage).Identity.ToString();
                root.Attr("Identifier", homePageIdentifier);
            }
            
            context.Document.Element("Orchard").Add(root);
        }

        private string Capitalize(string value) {
            if (String.IsNullOrEmpty(value))
                return value;

            return Char.ToUpper(value[0]) + value.Substring(1);
        }
    }
}


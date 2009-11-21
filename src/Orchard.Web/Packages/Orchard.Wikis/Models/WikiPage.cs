using System.Web.Routing;
using Orchard.Models;

namespace Orchard.Wikis.Models {
    public class WikiPage : ContentPartForRecord<WikiPageRecord>, IContentItemDisplay {

        string IContentItemDisplay.DisplayText {
            get { return Record.Name; }
        }

        RouteValueDictionary IContentItemDisplay.DisplayRouteValues() {
            return new RouteValueDictionary(new { area = "Orchard.Wikis", controller = "Page", action = "Show", id = ContentItem.Id });
        }

        RouteValueDictionary IContentItemDisplay.EditRouteValues() {
            return new RouteValueDictionary(new { area = "Orchard.Wikis", controller = "Page", action = "Edit", id = ContentItem.Id });
        }

    }
}
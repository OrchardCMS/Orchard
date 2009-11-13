using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.UI.Navigation {
    public class MenuItem {
        public string Text { get; set; }
        public string Position { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
        public IEnumerable<MenuItem> Items { get; set; }
    }
}
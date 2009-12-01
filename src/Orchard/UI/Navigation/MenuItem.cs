using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.Security.Permissions;

namespace Orchard.UI.Navigation {
    public class MenuItem {
        public MenuItem() {
            Permissions = Enumerable.Empty<Permission>();
        }

        public string Text { get; set; }
        public string Position { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
        public IEnumerable<MenuItem> Items { get; set; }
        public IEnumerable<Permission> Permissions { get; set; }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security.Permissions;

namespace Orchard.UI.Navigation {
    public class MenuItem {
        private IList<string> _classes = new List<string>();
        private IEnumerable<MenuItem> _items;

        public MenuItem() {
            Permissions = Enumerable.Empty<Permission>();
            LinkToFirstChild = true;
            Level = 1;
        }

        public LocalizedString Text { get; set; }
        public string IdHint { get; set; }
        public string Url { get; set; }
        public string Href { get; set; }
        public string Position { get; set; }
        public bool LinkToFirstChild { get; set; }
        public bool LocalNav { get; set; }
        public string Culture { get; set; }
        public bool Selected { get; set; }
        public int Level { get; set; }
        public RouteValueDictionary RouteValues { get; set; }

        public IEnumerable<MenuItem> Items {
            get { return _items; }
            set {
                _items = value;
                foreach (var item in _items) {
                    item.Level = Level + 1;
                }
            }
        }

        public IEnumerable<Permission> Permissions { get; set; }
        public IContent Content { get; set; }
        public IList<string> Classes {
            get { return _classes; } 
            set {
                if (value == null)
                    return;
                _classes = value;
            }
        }
   }
}
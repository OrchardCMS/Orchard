using System.Collections.Generic;
using System.Linq;

namespace Orchard.UI.Navigation {
    public class MenuItemComparer : IEqualityComparer<MenuItem> {
        public bool Equals(MenuItem x, MenuItem y) {
            if (!string.Equals(x.Text, y.Text)) {
                return false;
            }
            if (!string.Equals(x.Url, y.Url)) {
                return false;
            }
            if (x.RouteValues != null || y.RouteValues != null) {
                if (x.RouteValues == null || y.RouteValues == null) {
                    return false;
                }
                if (x.RouteValues.Keys.Any(key => y.RouteValues.ContainsKey(key) == false)) {
                    return false;
                }
                if (y.RouteValues.Keys.Any(key => x.RouteValues.ContainsKey(key) == false)) {
                    return false;
                }
                foreach (var key in x.RouteValues.Keys) {
                    if (!object.Equals(x.RouteValues[key], y.RouteValues[key])) {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(MenuItem obj) {
            var hash = 0;
            if (obj.Text != null) {
                hash ^= obj.Text.GetHashCode();
            }
            if (obj.Url != null) {
                hash ^= obj.Url.GetHashCode();
            }
            if (obj.RouteValues != null) {
                foreach (var item in obj.RouteValues) {
                    if (item.Key != null) {
                        hash ^= item.Key.GetHashCode();
                    }
                    if (item.Value != null) {
                        hash ^= item.Value.GetHashCode();
                    }
                }
            }
            return hash;
        }
    }
}
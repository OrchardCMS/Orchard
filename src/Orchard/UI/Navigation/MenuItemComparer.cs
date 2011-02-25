using System.Collections.Generic;
using System.Linq;

namespace Orchard.UI.Navigation {
    public class MenuItemComparer : IEqualityComparer<MenuItem> {
        public bool Equals(MenuItem x, MenuItem y) {
            if (!string.Equals(x.TextHint, y.TextHint)) {
                return false;
            }
            if (!string.IsNullOrWhiteSpace(x.Url) && !string.IsNullOrWhiteSpace(y.Url)) {
                if (!string.Equals(x.Url, y.Url)) {
                    return false;
                }
            }
            if (x.RouteValues != null && y.RouteValues != null) {
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
            if (obj.TextHint != null) {
                hash ^= obj.TextHint.GetHashCode();
            }
            return hash;
        }
    }
}
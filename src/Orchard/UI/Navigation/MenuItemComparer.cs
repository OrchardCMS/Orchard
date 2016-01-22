using System.Collections.Generic;
using System.Linq;

namespace Orchard.UI.Navigation {
    public class MenuItemComparer : IEqualityComparer<MenuItem> {
        public bool Equals(MenuItem x, MenuItem y) {
            var xTextHint = x.Text == null ? null : x.Text.TextHint;
            var yTextHint = y.Text == null ? null : y.Text.TextHint;
            if (!string.Equals(xTextHint, yTextHint)) {
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

            if (!string.IsNullOrWhiteSpace(x.Url) && y.RouteValues != null) {
                return false;
            }
            if (!string.IsNullOrWhiteSpace(y.Url) && x.RouteValues != null) {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(x.Position) && !string.IsNullOrWhiteSpace(y.Position)) {
                var xPosSplitted = x.Position.Split('.').Where(ss => ss != "0").ToArray();
                var yPosSplitted = y.Position.Split('.').Where(ss => ss != "0").ToArray();
                var xParentPosition = xPosSplitted.Length > 0 ? string.Join(".", xPosSplitted.Take(xPosSplitted.Length - 1)) : null;
                var yParentPosition = yPosSplitted.Length > 0 ? string.Join(".", yPosSplitted.Take(yPosSplitted.Length - 1)) : null;

                if (!string.Equals(xParentPosition, yParentPosition))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(MenuItem obj) {
            var hash = 0;
            if (obj.Text != null && obj.Text.TextHint != null) {
                hash ^= obj.Text.TextHint.GetHashCode();
            }
            return hash;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.UI;
using Orchard.UI.Navigation;

namespace Orchard.Utility {
    public static class Position {
        public static string GetNext(IEnumerable<MenuItem> menuItems) {

            var maxMenuItem = menuItems.Where(mi => PositionHasMajorNumber(mi)).OrderByDescending(mi => mi.Position, new FlatPositionComparer()).FirstOrDefault();

            // are there any menu item ?
            if (maxMenuItem != null) {

                var positionParts = maxMenuItem.Position.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries).Where(s => s.Trim().Length > 0).ToList();
                if (positionParts.Any()) {
                    int result;
                    if (int.TryParse(positionParts.ElementAt(0), out result)) {
                        return (result + 1).ToString(CultureInfo.InvariantCulture);
                    }
                }
            }

            return "1";
        }

        public static string GetNextMinor(int major, IEnumerable<MenuItem> menuItems) {
            var majorStr = major.ToString(CultureInfo.InvariantCulture);

            var maxMenuItem = menuItems.Where(mi => PositionHasMajorNumber(mi, major)).OrderByDescending(mi => mi.Position, new FlatPositionComparer()).FirstOrDefault();
            if (maxMenuItem == null) {
                // first one in this major position number
                return majorStr;
            }
            var positionParts = maxMenuItem.Position.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Where(s => s.Trim().Length > 0).ToList();
            if (positionParts.Count() > 1) {
                int result;
                if (int.TryParse(positionParts.ElementAt(1), out result)) {
                    return majorStr + "." + (result + 1);
                }
            }
            else if (positionParts.Count() == 1) {
                return majorStr + ".1";
            }

            return majorStr;
        }

        private static bool PositionHasMajorNumber(MenuItem mi, int? position = null) {
            int menuPosition;
            var major = mi.Position == null ? null : mi.Position.Split('.')[0];
            if (string.IsNullOrEmpty(major)) {
                return false;
            }
            var parsed = int.TryParse(major, out menuPosition);
            return parsed && (!position.HasValue || position.Value == menuPosition);
        }
    }
}
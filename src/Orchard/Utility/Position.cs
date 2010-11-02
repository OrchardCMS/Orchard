using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.UI;
using Orchard.UI.Navigation;

namespace Orchard.Utility {
    public static class Position {
        public static string GetNext(IEnumerable<MenuItem> menuItems) {
            var topMenuItem = menuItems.FirstOrDefault();

            if (topMenuItem != null) {
                var maxMenuItem = topMenuItem.Items.Where(PositionHasMojorNumber).OrderByDescending(mi => mi.Position, new FlatPositionComparer()).FirstOrDefault();
                var positionParts = maxMenuItem.Position.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries).Where(s => s.Trim() != "");
                if (positionParts.Count() > 0) {
                    int result;
                    if (int.TryParse(positionParts.ElementAt(0), out result)) {
                        return (result + 1).ToString();
                    }
                }
            }

            return "1";
        }

        private static bool PositionHasMojorNumber(MenuItem mi) {
            int foo;
            var major = mi.Position.Split('.')[0];
            return !string.IsNullOrEmpty(major) && int.TryParse(major, out foo);
        }
    }
}
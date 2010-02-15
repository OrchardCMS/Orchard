using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.UI.Navigation;

namespace Orchard.Utility {
    public static class Position {
        public static string GetNext(IEnumerable<MenuItem> menuItems) {
            //TODO: (erikpo) Clean up query to not hardcode to exclude 99, which is the admin menu item as of right now
            var maxMenuItem = menuItems.FirstOrDefault().Items.Where(mi => mi.Position != "99").OrderByDescending(mi => mi.Position, new PositionComparer()).FirstOrDefault();

            if (maxMenuItem != null) {
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
    }
}
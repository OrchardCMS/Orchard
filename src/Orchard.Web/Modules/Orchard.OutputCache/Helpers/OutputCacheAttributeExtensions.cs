using System.Linq;
using System.Web.Mvc;
using System.Web.UI;

namespace Orchard.OutputCache.Helpers {
    public static class OutputCacheAttributeExtensions {
        /// <summary>
        /// Returns true if the Location of the specified output cache attribute matches any of the specified list of locations.
        /// </summary>
        public static bool LocationIsIn(this OutputCacheAttribute attribute, params OutputCacheLocation[] locations) {
            return locations.Contains(attribute.Location);
        }
    }
}
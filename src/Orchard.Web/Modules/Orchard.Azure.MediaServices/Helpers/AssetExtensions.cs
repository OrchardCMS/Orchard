using System.Collections.Generic;
using System.Linq;
using Orchard.Azure.MediaServices.Models.Assets;

namespace Orchard.Azure.MediaServices.Helpers {
    public static class AssetExtensions {      
        public static IEnumerable<T> Filter<T>(this IEnumerable<Asset> assets) where T:Asset {
            return assets.Where(x => x is T).Cast<T>();
        }
    }
}
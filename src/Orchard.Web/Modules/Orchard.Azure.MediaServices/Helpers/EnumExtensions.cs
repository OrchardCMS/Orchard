using System.Linq;

namespace Orchard.Azure.MediaServices.Helpers {
    public static class EnumExtensions {      
        public static bool IsAny<T>(this T value, params T[] values) where T:struct {
            return values.Any(x => value.Equals(x));
        }

        public static bool IsNotAny<T>(this T value, params T[] values) where T:struct {
            return values.All(x => !value.Equals(x));
        }
    }
}
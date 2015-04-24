using System.Collections.Generic;

namespace Orchard.Layouts.Helpers {
    public static class ListHelper {
        
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items) {
            foreach (var item in items) {
                list.Add(item);
            }
        }
    }
}
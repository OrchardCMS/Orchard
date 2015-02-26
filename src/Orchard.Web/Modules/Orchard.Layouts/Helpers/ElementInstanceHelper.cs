using System.Collections.Generic;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Helpers {
    public static class ElementInstanceHelper {
        public static IEnumerable<Element> Flatten(this IEnumerable<Element> elements, int? levels = null) {
            var list = new List<Element>();
            Flatten(list, elements, levels);
            return list;
        }

        private static void Flatten(ICollection<Element> list, IEnumerable<Element> elements, int? levels = null) {
            foreach (var element in elements) {
                Flatten(list, element, 0, levels);
            }
        }

        private static void Flatten(ICollection<Element> list, Element element, int currentLevel, int? levels = null) {
            list.Add(element);
            var container = element as Container;

            if (container == null)
                return;

            if (levels != null && currentLevel == levels)
                return;

            foreach (var child in container.Elements) {
                Flatten(list, child, currentLevel + 1, levels);
            }
        }
    }
}
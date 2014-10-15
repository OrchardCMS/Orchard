using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Helpers {
    public static class ElementInstanceHelper {
        public static IEnumerable<IElement> Flatten(this IEnumerable<IElement> elements, int? levels = null) {
            var list = new List<IElement>();
            Flatten(list, elements, levels);
            return list;
        }

        private static void Flatten(ICollection<IElement> list, IEnumerable<IElement> elements, int? levels = null) {
            foreach (var element in elements) {
                Flatten(list, element, 0, levels);
            }
        }

        private static void Flatten(ICollection<IElement> list, IElement element, int currentLevel, int? levels = null) {
            list.Add(element);
            var container = element as IContainer;

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
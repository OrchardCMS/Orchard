using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Harvesters;

namespace Orchard.Layouts.Providers {
    public class DefaultCategoryProvider : Component, ICategoryProvider {
        public IEnumerable<Category> GetCategories() {
            yield return new Category("Content", T("Content"), T("Contains text elements"), -90);
            yield return new Category("Media", T("Media"), T("Contains media elements"), -80);
            yield return new Category("ContentParts", T("Parts"), T("Contains content parts"), -70);
            yield return new Category("ContentFields", T("Fields"), T("Contains content fields"), -69);
            yield return new Category("Widgets", T("Widgets"), T("Contains widgets"), -60);
            yield return new Category("Snippets", T("Snippets"), T("Contains snippet elements"), -50);
            yield return new Category("Layout", T("Layout"), T("Contains elements that help building layouts"), -40);
        }
    }
}
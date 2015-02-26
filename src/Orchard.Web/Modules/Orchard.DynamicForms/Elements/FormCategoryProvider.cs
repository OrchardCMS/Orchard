using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Harvesters;

namespace Orchard.DynamicForms.Elements {
    public class FormCategoryProvider : Component, ICategoryProvider {
        public IEnumerable<Category> GetCategories() {
            yield return new Category("Forms", T("Forms"), T("Contains elements that help building forms."), 10);
        }
    }
}
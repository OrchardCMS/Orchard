using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.FilterEditors.Forms;

namespace Orchard.Projections.FilterEditors
{
    public class BooleanFilterEditor : IFilterEditor
    {
        public BooleanFilterEditor()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanHandle(Type type)
        {
            return new[] {
                typeof(bool),
                typeof(bool?)
            }.Contains(type);
        }

        public string FormName => BooleanFilterForm.FormName;

        public Action<IHqlExpressionFactory> Filter(string property, dynamic formState)
        {
            return BooleanFilterForm.GetFilterPredicate(formState, property);
        }

        public LocalizedString Display(string property, dynamic formState)
        {
            return BooleanFilterForm.DisplayFilter(property, formState, T);
        }
    }
}
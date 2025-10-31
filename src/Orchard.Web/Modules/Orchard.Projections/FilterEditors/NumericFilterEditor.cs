using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.FilterEditors.Forms;

namespace Orchard.Projections.FilterEditors
{
    public class NumericFilterEditor : IFilterEditor
    {
        public NumericFilterEditor()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanHandle(Type type)
        {
            return new[] {
                typeof(byte),
                typeof(sbyte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
                typeof(float),
                typeof(double),
                typeof(decimal),
            }.Contains(type);
        }

        public string FormName => NumericFilterForm.FormName;

        public Action<IHqlExpressionFactory> Filter(string property, dynamic formState)
        {
            return NumericFilterForm.GetFilterPredicate(formState, property);
        }

        public LocalizedString Display(string property, dynamic formState)
        {
            return NumericFilterForm.DisplayFilter(property, formState, T);
        }
    }
}
using System;
using System.Linq;
using Orchard.Projections.ModelBinding;
using Orchard.Projections.PropertyEditors.Forms;

namespace Orchard.Projections.PropertyEditors
{
    public class NumericPropertyEditor : IPropertyEditor
    {
        private readonly IWorkContextAccessor _workContextAccessor;

        public NumericPropertyEditor(IWorkContextAccessor workContextAccessor)
        {
            _workContextAccessor = workContextAccessor;
        }

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

        public string FormName => NumericPropertyForm.FormName;

        public dynamic Format(dynamic display, object value, dynamic formState)
        {
            var culture = _workContextAccessor.GetContext().CurrentCulture;
            return NumericPropertyForm.FormatNumber(Convert.ToDecimal(value, new System.Globalization.CultureInfo(culture)), formState, culture);
        }
    }
}
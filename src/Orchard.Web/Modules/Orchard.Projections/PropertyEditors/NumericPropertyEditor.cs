using System;
using System.Linq;
using Orchard.Projections.ModelBinding;
using Orchard.Projections.PropertyEditors.Forms;

namespace Orchard.Projections.PropertyEditors {
    public class NumericPropertyEditor : IPropertyEditor {
        private readonly IWorkContextAccessor _workContextAccessor;

        public NumericPropertyEditor(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public bool CanHandle(Type type) {
            return new[] {
                typeof(Byte), 
                typeof(SByte), 
                typeof(Int16), 
                typeof(Int32), 
                typeof(Int64), 
                typeof(UInt16), 
                typeof(UInt32), 
                typeof(UInt64), 
                typeof(float), 
                typeof(double), 
                typeof(decimal), 
            }.Contains(type);
        }

        public string FormName {
            get { return NumericPropertyForm.FormName; }
        }

        public dynamic Format(dynamic display, object value, dynamic formState) {
            var culture = _workContextAccessor.GetContext().CurrentCulture;
            return NumericPropertyForm.FormatNumber(Convert.ToDecimal(value, new System.Globalization.CultureInfo(culture)), formState, culture);
        }
    }
}
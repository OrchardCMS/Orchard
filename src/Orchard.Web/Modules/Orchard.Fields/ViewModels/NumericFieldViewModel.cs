using Orchard.Fields.Fields;
using Orchard.Fields.Settings;
namespace Orchard.Fields.ViewModels {

    public class NumericFieldViewModel {
        public NumericField Field { get; set; }
        public NumericFieldSettings Settings { get; set; }
        public string Value { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Orchard.Core.Common.Settings {

    public class TextFieldSettings {
        [DataType("Flavor")]
        public string Flavor { get; set; }
        public bool Required { get; set; }
        public string Hint { get; set; }
    }
}

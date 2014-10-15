using System;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Layouts.Settings {
    public class LayoutPartSettings {
        public const string FlavorDefaultDefault = "textarea";
        private string _flavorDefault;

        [DataType("Flavor")]
        public string FlavorDefault {
            get { return !String.IsNullOrWhiteSpace(_flavorDefault) ? _flavorDefault : FlavorDefaultDefault; }
            set { _flavorDefault = value; }
        }
    }
}

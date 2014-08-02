using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Localization.Models {
    public class DateLocalizationOptions {

        public DateLocalizationOptions() {
            NullText = String.Empty;
            EnableTimeZoneConversion = true;
            EnableCalendarConversion = true;
        }
        
        /// <summary>
        /// Gets on sets the string to use in place of a null date when converting to and from a string. The default is an empty string.
        /// </summary>
        public string NullText { get; set; }
        
        /// <summary>
        /// Gets or sets a boolean indicating whether to perform time zone conversion as part of converting a date to and from a string. The default is true.
        /// </summary>
        public bool EnableTimeZoneConversion { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether to perform calendar conversion as part of converting a date to and from a string. The default is true.
        /// </summary>
        public bool EnableCalendarConversion { get; set; }
    }
}

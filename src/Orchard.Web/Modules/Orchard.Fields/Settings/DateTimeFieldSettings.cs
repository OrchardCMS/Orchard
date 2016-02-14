using Orchard.Core.Common.ViewModels;
using System;

namespace Orchard.Fields.Settings {

    public enum DateTimeFieldDisplays {
        DateAndTime,
        DateOnly,
        TimeOnly
    }

    public class DateTimeFieldSettings {
        public DateTimeFieldDisplays Display { get; set; }
        public string Hint { get; set; }
        public bool Required { get; set; }
        public string DatePlaceholder { get; set; }
        public string TimePlaceholder { get; set; }
        public DateTime? DefaultValue { get; set; }
        public DateTimeEditor Editor { get; set; }
    }
}

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
    }
}

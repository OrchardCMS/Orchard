using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using Orchard.Fields.Settings;

namespace Orchard.Fields.Fields {
    public class DateTimeField : ContentField {

        public DateTime DateTime {
            get {
                var settings = this.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                var value = Storage.Get<DateTime>();
                if (settings.Display == DateTimeFieldDisplays.DateOnly) {
                    return new DateTime(value.Year, value.Month, value.Day);
                }
                return value;
            }

            set {
                var settings = this.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                if (settings.Display == DateTimeFieldDisplays.DateOnly) {
                    Storage.Set(new DateTime(value.Year, value.Month, value.Day));
                }
                else {
                    Storage.Set(value);
                }
            }
        }

        public DateTimeFieldDisplays Display {
            get {
                var settings = this.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                return settings.Display;
            }
        }
    } 
}

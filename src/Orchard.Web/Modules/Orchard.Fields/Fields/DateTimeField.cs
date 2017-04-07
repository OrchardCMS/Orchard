using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using Orchard.Fields.Settings;

namespace Orchard.Fields.Fields {
    public class DateTimeField : ContentField {

        public DateTime? DateTime {
            get {
                var settings = this.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                var value = Storage.Get<DateTime?>();
                if (value.HasValue) {
                  if (settings.Display == DateTimeFieldDisplays.DateOnly) {
                      return new DateTime(value.Value.Year, value.Value.Month, value.Value.Day);
                  }
                }
                return value;
            }

            set {
                if (value.HasValue) {
                  var settings = this.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                  if (settings.Display == DateTimeFieldDisplays.DateOnly) {
                      Storage.Set(new DateTime(value.Value.Year, value.Value.Month, value.Value.Day));
                  }
                  else {
                      Storage.Set(value.Value);
                  }
                }
                else {
                  Storage.Set<System.DateTime?>(null);
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

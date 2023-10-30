using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using Orchard.Fields.Settings;
using Orchard.UI.Notify;

namespace Orchard.Fields.Fields {
    public class DateTimeField : ContentField {

        public DateTime DateTime {
            get {
                var settings = this.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                var value = Storage.Get<DateTime>();
                if (Display == DateTimeFieldDisplays.DateOnly) {
                    return new DateTime(value.Year, value.Month, value.Day);
                }
                return value;
            }

            set {
                var settings = this.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                if (Display == DateTimeFieldDisplays.DateOnly) {
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
                if (settings.AllowDisplayOptionsOverride) {
                    if (Enum.TryParse(DisplayOption, out DateTimeFieldDisplays displayOption)){
                        return displayOption;
                    }                    
                }                
                return settings.Display;
            }
            set {
                DisplayOption = value.ToString();                
            }
        }

        protected string DisplayOption {
            get {
                return Storage.Get<string>("DisplayOption");                
            }
            set {
                Storage.Set("DisplayOption", value ?? String.Empty);
            }
        }
    } 
}

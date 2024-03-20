using Orchard.Core.Common.ViewModels;
using Orchard.Fields.Settings;


namespace Orchard.Fields.ViewModels {
    public class DateTimeFieldViewModel {
        public string Name { get; set; }
        public string Hint { get; set; }
        public bool IsRequired { get; set; }
        public DateTimeEditor Editor { get; set; }
        public bool AllowDisplayOptionsOverride { get; set; }
        public DateTimeFieldDisplays DisplayOption { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Core.Common.ViewModels {
    public class DateTimeEditor {
        public string Date { get; set; }
        public string Time { get; set; }
        public bool ShowDate { get; set; }
        public bool ShowTime { get; set; }
        public string DatePlaceholder { get; set; }
        public string TimePlaceholder { get; set; }
    }
}
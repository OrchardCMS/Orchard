using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.ImportExport.ViewModels {
    public class RecurringTaskViewModel {
        public bool IsActive { get; set; }
        public virtual int RepeatFrequencyInMinutes { get; set; }

        public SelectList RepeatFreqencyOptions {
            get {
                var items = new List<SelectListItem>();
                items.AddRange(GetRange(1, 5, 1, min => string.Format("{0} minutes", min)));
                items.AddRange(GetRange(5, 25, 5, min => string.Format("{0} minutes", min)));
                items.AddRange(GetRange(30, 90, 15, min => string.Format("{0} minutes", min)));
                items.AddRange(GetRange(120, 720, 60, min => string.Format("{0} hours", min / 60)));
                return new SelectList(items, "Value", "Text");
            }
        }

        private static IEnumerable<SelectListItem> GetRange(int rangeStart, int rangeEnd, int increment, Func<int, string> display) {
            var items = new List<SelectListItem>();
            for (var i = rangeStart; i <= rangeEnd; i += increment) {
                items.Add(new SelectListItem {
                    Text = string.Format("{0}", display(i)),
                    Value = i.ToString()
                });
            }
            return items;
        }
    }
}

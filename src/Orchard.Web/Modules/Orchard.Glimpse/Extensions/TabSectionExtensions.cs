using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Tab.Assist;
using Orchard.Glimpse.Models;

namespace Orchard.Glimpse.Extensions {
    public static class TabSectionExtensions {
        public static void AddTimingSummary(this TabSection section, IEnumerable<IDurationMessage> messages) {
            if (!section.Rows.Any()) {
                return;
            }

            var columnCount = section.Rows.First().Columns.Count();

            var row = section.AddRow();

            var itemCount = messages.Count();
            row.Column($"{itemCount} item{(itemCount == 1 ? "" : "s")}");

            for (int i = 0; i < columnCount - 3; i++) {
                row.Column("");
            }

            row.Column("Total time:");
            row.Column(messages.Sum(m => m.Duration.TotalMilliseconds).ToTimingString());
            row.Selected();
        }
    }
}
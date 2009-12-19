using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.UI.Zones {
    public class ZoneManager : IZoneManager {
        public void Render<TModel>(HtmlHelper<TModel> html, ZoneCollection zones, string zoneName, string partitions) {

            ZoneEntry zone;
            if (!zones.TryGetValue(zoneName, out zone))
                return;

            var groups = BuildGroups(partitions, zone);

            foreach (var item in groups.SelectMany(x => x.Items)) {
                item.WasExecuted = true;
                item.Execute(html);
            }

        }

        private IEnumerable<Group> BuildGroups(string partitions, ZoneEntry zone) {

            var partitionCodes = (":before " + partitions + " :* :after").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var itemsRemaining = zone.Items.Where(x => x.WasExecuted == false);

            Group catchAllItem = null;

            var results = new List<Group>();
            foreach (var code in partitionCodes) {
                if (code == ":*") {
                    catchAllItem = new Group();
                    results.Add(catchAllItem);
                }
                else {
                    var value = code;
                    var items = itemsRemaining.Where(x => (":" + x.Position).StartsWith(value));
                    results.Add(new Group { Items = items.ToArray() });
                    itemsRemaining = itemsRemaining.Except(items).ToArray();
                }
            }
            if (catchAllItem != null) {
                catchAllItem.Items = itemsRemaining;
            }
            return results;
        }

        class Group {
            public IEnumerable<ZoneItem> Items { get; set; }
        }
    }
}

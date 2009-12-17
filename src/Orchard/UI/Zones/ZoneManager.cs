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

            var partitionItems = LocatePartitionItems(partitions, zone);

            foreach (var partitionItem in partitionItems) {
                foreach (var item in zone.Items) {
                    if (item.WasExecuted)
                        continue;

                    item.WasExecuted = true;
                    item.Execute(html);
                }

            }
        }

        private IEnumerable<PartitionItem> LocatePartitionItems(string partitions, ZoneEntry zone) {

            var partitionCodes = (":before " + partitions + " :* :after").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var itemsRemaining = zone.Items.Where(x => x.WasExecuted == false);

            PartitionItem catchAllItem = null;

            var results = new List<PartitionItem>();
            foreach (var code in partitionCodes) {
                if (code == ":*") {
                    catchAllItem=new PartitionItem();
                    results.Add(catchAllItem);
                }
                else {
                    var value = code;
                    var items = itemsRemaining.Where(x => (":" + x.Position).StartsWith(value));
                    results.Add(new PartitionItem { ZoneItems = items.ToArray() });
                    itemsRemaining = itemsRemaining.Except(items).ToArray();
                }
            }
            if (catchAllItem != null) {
                catchAllItem.ZoneItems = itemsRemaining;
            }
            return results;
        }

        class PartitionItem {
            public IEnumerable<ZoneItem> ZoneItems { get; set; }
        }
    }
}

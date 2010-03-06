using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Logging;
using Orchard.UI.Navigation;

namespace Orchard.UI.Zones {

    public class ZoneManager : IZoneManager {
        private readonly IEnumerable<IZoneManagerEvents> _events;

        public ZoneManager(IEnumerable<IZoneManagerEvents> events) {
            _events = events;
            Logger = NullLogger.Instance;
        }

        public void Render<TModel>(HtmlHelper<TModel> html, ZoneCollection zones, string zoneName, string partitions, string[] exclude) {
            IEnumerable<Group> groups;
            if (string.IsNullOrEmpty(zoneName)) {
                var entries = zones.Values.Where(z => !exclude.Contains(z.ZoneName));
                groups = BuildGroups(partitions, entries);
            }
            else {
                ZoneEntry entry;
                if (zones.TryGetValue(zoneName, out entry)) {
                    groups = BuildGroups(partitions, new[] { entry });
                }
                else {
                    groups = Enumerable.Empty<Group>();
                }
            }

            var context = new ZoneRenderContext {
                Html = html,
                Zones = zones,
                ZoneName = zoneName,
                RenderingItems = groups.SelectMany(x => x.Items).ToList()
            };

            _events.Invoke(x => x.ZoneRendering(context), Logger);
            foreach (var item in context.RenderingItems) {
                var zoneItem = item;
                _events.Invoke(x => x.ZoneItemRendering(context, zoneItem), Logger);
                zoneItem.WasExecuted = true;
                zoneItem.Execute(html);
                _events.Invoke(x => x.ZoneItemRendered(context, zoneItem), Logger);
            }
            _events.Invoke(x => x.ZoneRendered(context), Logger);

        }

        protected ILogger Logger { get; set; }

        private IEnumerable<Group> BuildGroups(string partitions, IEnumerable<ZoneEntry> zones) {

            var partitionCodes = (":before " + partitions + " :* :after").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var itemsRemaining = zones.SelectMany(zone => zone.Items.Where(x => x.WasExecuted == false));

            Group catchAllItem = null;

            var positionComparer = new PositionComparer();
            var results = new List<Group>();
            foreach (var code in partitionCodes) {
                if (code == ":*") {
                    catchAllItem = new Group();
                    results.Add(catchAllItem);
                }
                else {
                    var value = code;
                    var items = itemsRemaining
                        .Where(x => (":" + x.Position).StartsWith(value))
                        .OrderBy(x => x.Position, positionComparer);
                    results.Add(new Group { Items = items.ToArray() });
                    itemsRemaining = itemsRemaining.Except(items).ToArray();
                }
            }
            if (catchAllItem != null) {
                catchAllItem.Items = itemsRemaining
                    .OrderBy(x => x.Position, positionComparer)
                    .ToArray();
            }
            return results;
        }

        class Group {
            public IEnumerable<ZoneItem> Items { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Orchard.UI.Zones {
    public class ZoneManager {
        public void Render<TModel>(HtmlHelper<TModel> html, ZoneCollection zones, string zoneName, string partitions) {

            ZoneEntry zone;
            if (!zones.TryGetValue(zoneName, out zone))
                return;

            //TODO: partitions
            foreach (var item in zone.Items) {
                if (item.WasExecuted)
                    continue;

                item.WasExecuted = true;
                item.Execute(html);
            }
        }
    }
}

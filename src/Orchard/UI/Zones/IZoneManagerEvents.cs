using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.UI.Zones {
    public interface IZoneManagerEvents : IEvents {
        void ZoneRendering(ZoneRenderContext context);
        void ZoneItemRendering(ZoneRenderContext context, ZoneItem item);
        void ZoneItemRendered(ZoneRenderContext context, ZoneItem item);
        void ZoneRendered(ZoneRenderContext context);
    }
    public class ZoneRenderContext {
        public HtmlHelper Html { get; set; }
        public ZoneCollection Zones { get; set; }
        public string ZoneName { get; set; }
        public IEnumerable<ZoneItem> RenderingItems { get; set; }
    }

}
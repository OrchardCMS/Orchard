using Orchard.Localization;
using Orchard.UI.Zones;

namespace Orchard.Themes.Services {
#if REFACTORING
    public class ThemeZoneManagerEvents : IZoneManagerEvents {
        public ThemeZoneManagerEvents() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void ZoneRendering(ZoneRenderContext context) {
#if DEBUG
            context.Html.ViewContext.Writer.WriteLine(T("<!-- begin zone: {0} -->", LocalizedString.TextOrDefault(context.ZoneName, T("etc. (ZonesAny)"))));
#endif
        }

        public void ZoneItemRendering(ZoneRenderContext context, ZoneItem item) {
#if DEBUG
            //info: doesn't cover all ZoneItem types
            var writer = context.Html.ViewContext.Writer;
            if (item is RenderPartialZoneItem)
                writer.WriteLine(T("<!-- begin: {0} -->", (item as RenderPartialZoneItem).TemplateName));
            else if (item is ContentPartDisplayZoneItem)
                writer.WriteLine(T("<!-- begin: {0} -->", (item as ContentPartDisplayZoneItem).TemplateName));
            else if (item is ContentPartEditorZoneItem)
                writer.WriteLine(T("<!-- begin: {0} -->", (item as ContentPartEditorZoneItem).TemplateName));
#endif
        }

        public void ZoneItemRendered(ZoneRenderContext context, ZoneItem item) {
#if DEBUG
            //info: doesn't cover all ZoneItem types
            var writer = context.Html.ViewContext.Writer;
            if (item is RenderPartialZoneItem)
                writer.WriteLine(T("<!-- end: {0} -->", (item as RenderPartialZoneItem).TemplateName));
            else if (item is ContentPartDisplayZoneItem)
                writer.WriteLine(T("<!-- end: {0} -->", (item as ContentPartDisplayZoneItem).TemplateName));
            else if (item is ContentPartEditorZoneItem)
                writer.WriteLine(T("<!-- end: {0} -->", (item as ContentPartEditorZoneItem).TemplateName));
#endif
        }

        public void ZoneRendered(ZoneRenderContext context) {
#if DEBUG
            context.Html.ViewContext.Writer.WriteLine(T("<!-- end zone: {0} -->", LocalizedString.TextOrDefault(context.ZoneName, T("etc. (ZonesAny)"))));
#endif
        }
    }
#endif
}
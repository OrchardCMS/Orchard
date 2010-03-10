using System.IO;
using System.Linq;
using System.Web.Mvc.Html;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Zones;

namespace Orchard.Core.Themes.DesignerNotes {
    public class ZoneManagerEvents : IZoneManagerEvents {
        private readonly IThemeService _themeService;
        private readonly IAuthorizationService _authorizationService;

        public ZoneManagerEvents(IThemeService themeService, IAuthorizationService authorizationService) {
            _themeService = themeService;
            _authorizationService = authorizationService;
        }

        public virtual IUser CurrentUser { get; set; }

        public void ZoneRendering(ZoneRenderContext context) {
            if (context.RenderingItems.Any())
                return;

            var requestContext = context.Html.ViewContext.RequestContext;
            var theme = _themeService.GetRequestTheme(requestContext);
            var virtualPath = "~/Themes/" + theme.ThemeName + "/Zones/" + context.ZoneName + ".html";
            var physicalPath = requestContext.HttpContext.Server.MapPath(virtualPath);
            if (!File.Exists(physicalPath))
                return;

            var accessAdminPanel = _authorizationService.TryCheckAccess(
                StandardPermissions.AccessAdminPanel, CurrentUser, null);

            var writer = context.Html.ViewContext.Writer;
            if (accessAdminPanel) {
                writer.Write("<div class=\"managewrapper\"><div class=\"manage\">");
                writer.Write(context.Html.ActionLink("Edit", "AddWidget", new {
                    Area = "Futures.Widgets",
                    Controller = "Admin",
                    context.ZoneName,
                    theme.ThemeName,
                    ReturnUrl = requestContext.HttpContext.Request.Url,
                }));
                writer.Write("</div>");
            }
            writer.Write(File.ReadAllText(physicalPath));
            if (accessAdminPanel) {
                writer.Write("</div>");
            }
        }

        public void ZoneItemRendering(ZoneRenderContext context, ZoneItem item) {
        }

        public void ZoneItemRendered(ZoneRenderContext context, ZoneItem item) {
        }

        public void ZoneRendered(ZoneRenderContext context) {
        }
    }
}

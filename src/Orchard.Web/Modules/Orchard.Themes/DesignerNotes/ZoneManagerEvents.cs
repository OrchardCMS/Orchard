using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc.Html;
using Orchard.Environment.Descriptor;
using Orchard.Security;
using Orchard.Services;
using Orchard.UI.Zones;

namespace Orchard.Themes.DesignerNotes {
    public class ZoneManagerEvents : IZoneManagerEvents {
        private readonly IThemeService _themeService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        public ZoneManagerEvents(IThemeService themeService, 
            IAuthorizationService authorizationService, 
            IShellDescriptorManager shellDescriptorManager,
            IEnumerable<IHtmlFilter> htmlFilters) {

            _themeService = themeService;
            _authorizationService = authorizationService;
            _shellDescriptorManager = shellDescriptorManager;
            _htmlFilters = htmlFilters;
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
            if (accessAdminPanel) {
                //Temporary: Don't show "edit" button if "Futures.Widgets" is not enabled.
                accessAdminPanel = _shellDescriptorManager
                    .GetShellDescriptor()
                    .Features
                    .Any(f => f.Name == "Futures.Widgets");
            }

            var writer = context.Html.ViewContext.Writer;
            if (accessAdminPanel) {
                writer.Write("<div class=\"managewrapper\"><div class=\"manage\">");
                writer.Write(context.Html.ActionLink("Edit", "AddWidget", new {
                                                                                  Area = "Futures.Widgets",
                                                                                  Controller = "Admin",
                                                                                  context.ZoneName,
                                                                                  theme.ThemeName,
                                                                                  ReturnUrl = requestContext.HttpContext.Request.RawUrl,
                                                                              }));
                writer.Write("</div>");
            }

            var fileText = _htmlFilters
                .Aggregate(File.ReadAllText(physicalPath), (text, filter) => filter.ProcessContent(text));

            writer.Write(fileText);
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
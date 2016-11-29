using System;
using System.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Services {
    public class WidgetCommandsService : IWidgetCommandsService {
        private readonly IMenuService _menuService;
        private readonly IWidgetsService _widgetsService;
        private readonly ISiteService _siteService;
        private readonly IMembershipService _membershipService;
        private readonly IContentManager _contentManager;

        private const string LoremIpsum = "<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur a nibh ut tortor dapibus vestibulum. Aliquam vel sem nibh. Suspendisse vel condimentum tellus.</p>";

        public WidgetCommandsService(
            IWidgetsService widgetsService,
            IMenuService menuService,
            ISiteService siteService,
            IMembershipService membershipService,
            IContentManager contentManager) {
            _siteService = siteService;
            _membershipService = membershipService;
            _widgetsService = widgetsService;
            _menuService = menuService;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public WidgetPart CreateBaseWidget(CommandContext context, string type, string title, string name, string zone, string position, string layer, string identity, bool renderTitle, string owner, string text, bool useLoremIpsumText, string menuName) {
            var widgetTypeNames = _widgetsService.GetWidgetTypeNames().ToList();
            if (!widgetTypeNames.Contains(type)) {
                context.Output.WriteLine(T("Creating widget failed : type {0} was not found. Supported widget types are: {1}.",
                    type,
                    string.Join(" ", widgetTypeNames)));
                return null;
            }

            var layerPart = GetLayer(layer);
            if (layerPart == null) {
                context.Output.WriteLine(T("Creating widget failed : layer {0} was not found.", layer));
                return null;
            }

            var widget = _widgetsService.CreateWidget(layerPart.ContentItem.Id, type, T(title).Text, position, zone);

            if (!String.IsNullOrWhiteSpace(name)) {
                widget.Name = name.Trim();
            }

            var widgetText = String.Empty;
            if (widget.Has<BodyPart>()) {
                if (useLoremIpsumText) {
                    widgetText = T(LoremIpsum).Text;
                }
                else {
                    if (!String.IsNullOrEmpty(text)) {
                        widgetText = text;
                    }
                }
                widget.As<BodyPart>().Text = text;
            }

            widget.RenderTitle = renderTitle;

            if (widget.Has<MenuWidgetPart>() && !String.IsNullOrWhiteSpace(menuName)) {
                var menu = _menuService.GetMenu(menuName);

                if (menu != null) {
                    widget.RenderTitle = false;
                    widget.As<MenuWidgetPart>().MenuContentItemId = menu.ContentItem.Id;
                }
            }

            if (String.IsNullOrEmpty(owner)) {
                owner = _siteService.GetSiteSettings().SuperUser;
            }
            var widgetOwner = _membershipService.GetUser(owner);
            widget.As<ICommonPart>().Owner = widgetOwner;

            if (widget.Has<IdentityPart>() && !String.IsNullOrEmpty(identity)) {
                widget.As<IdentityPart>().Identifier = identity;
            }

            return widget;
        }
        private LayerPart GetLayer(string layer) {
            var layers = _widgetsService.GetLayers();
            return layers.FirstOrDefault(layerPart => String.Equals(layerPart.Name, layer, StringComparison.OrdinalIgnoreCase));
        }

        public void Publish(WidgetPart widget) {
            _contentManager.Publish(widget.ContentItem);
        }
    }
}
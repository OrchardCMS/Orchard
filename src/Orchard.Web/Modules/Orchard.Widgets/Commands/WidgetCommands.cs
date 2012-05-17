using System;
using System.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Title.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.Commands {
    public class WidgetCommands : DefaultOrchardCommandHandler {
        private readonly IWidgetsService _widgetsService;
        private readonly ISiteService _siteService;
        private readonly IMembershipService _membershipService;
        private readonly IContentManager _contentManager;
        private readonly IMenuService _menuService;
        private const string LoremIpsum = "<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur a nibh ut tortor dapibus vestibulum. Aliquam vel sem nibh. Suspendisse vel condimentum tellus.</p>";

        public WidgetCommands(
            IWidgetsService widgetsService, 
            ISiteService siteService, 
            IMembershipService membershipService,
            IContentManager contentManager,
            IMenuService menuService) {
            _widgetsService = widgetsService;
            _siteService = siteService;
            _membershipService = membershipService;
            _contentManager = contentManager;
            _menuService = menuService;

            RenderTitle = true;
        }

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public bool RenderTitle { get; set; }

        [OrchardSwitch]
        public string Zone { get; set; }

        [OrchardSwitch]
        public string Position { get; set; }

        [OrchardSwitch]
        public string Layer { get; set; }

        [OrchardSwitch]
        public string Identity { get; set; }

        [OrchardSwitch]
        public string Owner { get; set; }

        [OrchardSwitch]
        public string Text { get; set; }

        [OrchardSwitch]
        public bool UseLoremIpsumText { get; set; }

        [OrchardSwitch]
        public bool Publish { get; set; }

        [OrchardSwitch]
        public string MenuName { get; set; }

        [CommandName("widget create")]
        [CommandHelp("widget create <type> /Title:<title> /Zone:<zone> /Position:<position> /Layer:<layer> [/Identity:<identity>] [/RenderTitle:true|false] [/Owner:<owner>] [/Text:<text>] [/UseLoremIpsumText:true|false] [/MenuName:<name>]\r\n\t" + "Creates a new widget")]
        [OrchardSwitches("Title,Zone,Position,Layer,Identity,Owner,Text,UseLoremIpsumText,MenuName,RenderTitle")]
        public void Create(string type) {
            var widgetTypeNames = _widgetsService.GetWidgetTypeNames();
            if (!widgetTypeNames.Contains(type)) {
                Context.Output.WriteLine(T("Creating widget failed : type {0} was not found. Supported widget types are: {1}.", 
                    type,
                    widgetTypeNames.Aggregate(String.Empty, (current, widgetType) => current + " " + widgetType)));
                return;
            }

            var layer = GetLayer(Layer);
            if (layer == null) {
                Context.Output.WriteLine(T("Creating widget failed : layer {0} was not found.", Layer));
                return;
            }

            var widget = _widgetsService.CreateWidget(layer.ContentItem.Id, type, T(Title).Text, Position, Zone);
            var text = String.Empty;
            if (widget.Has<BodyPart>()) {
                if (UseLoremIpsumText) {
                    text = T(LoremIpsum).Text;
                }
                else {
                    if (!String.IsNullOrEmpty(Text)) {
                        text = Text;
                    }
                }
                widget.As<BodyPart>().Text = text;
            }

            widget.RenderTitle = RenderTitle;

            if(widget.Has<MenuWidgetPart>() && !String.IsNullOrWhiteSpace(MenuName)) {
                // flushes before doing a query in case a previous command created the menu
                _contentManager.Flush();

                var menu = _menuService.GetMenu(MenuName);
                
                if(menu != null) {
                    widget.RenderTitle = false;
                    widget.As<MenuWidgetPart>().Menu = menu.ContentItem.Record;
                }
            }

            if (String.IsNullOrEmpty(Owner)) {
                Owner = _siteService.GetSiteSettings().SuperUser;
            }
            var owner = _membershipService.GetUser(Owner);
            widget.As<ICommonPart>().Owner = owner;

            if (widget.Has<IdentityPart>() && !String.IsNullOrEmpty(Identity)) {
                widget.As<IdentityPart>().Identifier = Identity;
            }

            Context.Output.WriteLine(T("Widget created successfully.").Text);
        }

        private LayerPart GetLayer(string layer) {
            var layers = _widgetsService.GetLayers();
            return layers.FirstOrDefault(layerPart => String.Equals(layerPart.Name, layer, StringComparison.OrdinalIgnoreCase));
        }
    }
}
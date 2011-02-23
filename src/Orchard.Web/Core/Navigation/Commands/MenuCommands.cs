using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;

namespace Orchard.Core.Navigation.Commands {
    public class MenuCommands : DefaultOrchardCommandHandler {
        private readonly IContentManager _contentManager;

        public MenuCommands(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        [OrchardSwitch]
        public string MenuPosition { get; set; }

        [OrchardSwitch]
        public string MenuText { get; set; }

        [OrchardSwitch]
        public string Url { get; set; }

        [OrchardSwitch]
        public bool OnMainMenu { get; set; }

        [CommandName("menuitem create")]
        [CommandHelp("menuitem create /MenuPosition:<position> /MenuText:<text> /Url:<url> [/OnMainMenu:true|false]\r\n\t" + "Creates a new menu item")]
        [OrchardSwitches("MenuPosition,MenuText,Url,OnMainMenu")]
        public void Create() {
            var menuItem = _contentManager.Create("MenuItem");
            menuItem.As<MenuPart>().MenuPosition = MenuPosition;
            menuItem.As<MenuPart>().MenuText = T(MenuText).ToString();
            menuItem.As<MenuPart>().OnMainMenu = OnMainMenu;
            menuItem.As<MenuItemPart>().Url = Url;

            Context.Output.WriteLine(T("Menu item created successfully.").Text);
        }
    }
}
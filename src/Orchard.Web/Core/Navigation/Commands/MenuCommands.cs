using System.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Title.Models;

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
        public string MenuName { get; set; }

        [CommandName("menuitem create")]
        [CommandHelp("menuitem create /MenuPosition:<position> /MenuText:<text> /Url:<url> /MenuName:<name>\r\n\t" + "Creates a new menu item")]
        [OrchardSwitches("MenuPosition,MenuText,Url,MenuName")]
        public void Create() {
            // flushes before doing a query in case a previous command created the menu
            _contentManager.Flush();

            var menu = _contentManager.Query<TitlePart, TitlePartRecord>()
                .Where(x => x.Title == MenuName)
                .ForType("Menu")
                .Slice(0, 1)
                .FirstOrDefault();

            if(menu == null) {
                Context.Output.WriteLine(T("Menu not found.").Text);
                return;
            }

            var menuItem = _contentManager.Create("MenuItem");
            menuItem.As<MenuPart>().MenuPosition = MenuPosition;
            menuItem.As<MenuPart>().MenuText = T(MenuText).ToString();
            menuItem.As<MenuPart>().MenuRecord = menu.ContentItem.Record;
            menuItem.As<MenuItemPart>().Url = Url;

            Context.Output.WriteLine(T("Menu item created successfully.").Text);
        }

        [CommandName("menu create")]
        [CommandHelp("menu create /MenuName:<name>\r\n\t" + "Creates a new menu")]
        [OrchardSwitches("MenuName")]
        public void CreateMenu() {
            if (string.IsNullOrWhiteSpace(MenuName)) {
                Context.Output.WriteLine(T("Menu name can't be empty.").Text);
                return;
            }

            var menu = _contentManager.Create("Menu");
            menu.As<TitlePart>().Title = MenuName;

            Context.Output.WriteLine(T("Menu created successfully.").Text);
        }
    }
}
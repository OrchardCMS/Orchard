using System;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Core.Title.Models;
using Orchard.UI.Navigation;
using Orchard.Utility;

namespace Orchard.Pages.Commands {
    public class PageCommands : DefaultOrchardCommandHandler {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly ISiteService _siteService;
        private readonly IMenuService _menuService;
        private readonly INavigationManager _navigationManager;
        private readonly IAuthenticationService _authenticationService;

        public PageCommands(
            IContentManager contentManager, 
            IMembershipService membershipService, 
            IAuthenticationService authenticationService,
            ISiteService siteService,
            IMenuService menuService,
            INavigationManager navigationManager) {
            _contentManager = contentManager;
            _membershipService = membershipService;
            _siteService = siteService;
            _menuService = menuService;
            _navigationManager = navigationManager;
            _authenticationService = authenticationService;
        }

        [OrchardSwitch]
        public string Slug { get; set; }

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public string Path { get; set; }

        [OrchardSwitch]
        public string Text { get; set; }

        [OrchardSwitch]
        public string Owner { get; set; }

        [OrchardSwitch]
        public bool Homepage { get; set; }

        [OrchardSwitch]
        public bool Publish { get; set; }

        [OrchardSwitch]
        public bool UseWelcomeText { get; set; }

        [OrchardSwitch]
        public string MenuText { get; set; }

        [OrchardSwitch]
        public string MenuName { get; set; }

        [CommandName("page create")]
        [CommandHelp("page create [/Slug:<slug>] /Title:<title> /Path:<path> [/Text:<text>] [/Owner:<username>] [/MenuName:<name>] [/MenuText:<menu text>] [/Homepage:true|false] [/Publish:true|false] [/UseWelcomeText:true|false]\r\n\t" + "Creates a new page")]
        [OrchardSwitches("Slug,Title,Path,Text,Owner,MenuText,Homepage,MenuName,Publish,UseWelcomeText")]
        public void Create() {
            _contentManager.Flush();

            if (String.IsNullOrEmpty(Owner)) {
                Owner = _siteService.GetSiteSettings().SuperUser;
            }

            var owner = _membershipService.GetUser(Owner);
            _authenticationService.SetAuthenticatedUserForRequest(owner);

            var page = _contentManager.Create("Page", VersionOptions.Draft);
            page.As<TitlePart>().Title = Title;
            page.As<ICommonPart>().Owner = owner;

            if (!String.IsNullOrWhiteSpace(MenuText)) {
                var menu = _menuService.GetMenu(MenuName);

                if (menu != null) {
                    var menuItem = _contentManager.Create<ContentMenuItemPart>("ContentMenuItem");
                    menuItem.Content = page;
                    menuItem.As<MenuPart>().MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));
                    menuItem.As<MenuPart>().MenuText = MenuText;
                    menuItem.As<MenuPart>().Menu = menu;
                }
            }

            // (PH:Autoroute) Hackish way to leave Slug and Homepage switches intact without requiring a dependency on Autoroute. This may throw an Exception with
            // no AutoroutePart. But it means that normal setup recipes will still be able to give you a homepage without issue.
            if (Homepage || !String.IsNullOrWhiteSpace(Slug)) {
                dynamic dpage = page;
                if (dpage.AutoroutePart != null) {
                    dpage.AutoroutePart.UseCustomPattern = true;
                    dpage.AutoroutePart.CustomPattern = Homepage ? "/" : Slug;
                }
            }

            var text = String.Empty;
            if (UseWelcomeText) {
                text = T(
@"<p>You've successfully setup your Orchard Site and this is the homepage of your new site.
Here are a few things you can look at to get familiar with the application.
Once you feel confident you don't need this anymore, you can
<a href=""Admin/Contents/Edit/{0}"">remove it by going into editing mode</a>
and replacing it with whatever you want.</p>
<p>First things first - You'll probably want to <a href=""Admin/Settings"">manage your settings</a>
and configure Orchard to your liking. After that, you can head over to
<a href=""Admin/Themes"">manage themes to change or install new themes</a>
and really make it your own. Once you're happy with a look and feel, it's time for some content.
You can start creating new custom content types or start from the built-in ones by
<a href=""Admin/Contents/Create/Page"">adding a page</a>, or <a href=""Admin/Navigation"">managing your menus.</a></p>
<p>Finally, Orchard has been designed to be extended. It comes with a few built-in
modules such as pages and blogs or themes. If you're looking to add additional functionality,
you can do so by creating your own module or by installing one that somebody else built.
Modules are created by other users of Orchard just like you so if you feel up to it,
<a href=""http://orchardproject.net/contribution"">please consider participating</a>.</p>
<p>Thanks for using Orchard – The Orchard Team </p>", page.Id).Text;
            }
            else {
                if (!String.IsNullOrEmpty(Text)) {
                    text = Text;
                }
            }
            page.As<BodyPart>().Text = text;
            if (Publish) {
                _contentManager.Publish(page);
            }

            Context.Output.WriteLine(T("Page created successfully.").Text);
        }
    }
}
using System;
using System.Web.Mvc;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Setup.ViewModels;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace Orchard.Setup.Controllers {
    public class SetupController : Controller {
        private readonly INotifier _notifier;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsLoader _shellSettingsLoader;
        private readonly IAppDataFolder _appDataFolder;

        public SetupController(
            INotifier notifier,
            IOrchardHost orchardHost, 
            IShellSettingsLoader shellSettingsLoader,
            IAppDataFolder appDataFolder) {
            _notifier = notifier;
            _orchardHost = orchardHost;
            _shellSettingsLoader = shellSettingsLoader;
            _appDataFolder = appDataFolder;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        private ActionResult IndexViewResult(SetupViewModel model) {
            string message;
            if (!CanWriteToAppDataFolder(out message)) {
                _notifier.Error(
                    T(
                        "Hey, it looks like I can't write to the App_Data folder in the root of this application and that's where I need to save some of the information you're about to enter.\r\n\r\nPlease give me (the machine account this application is running under) write access to App_Data so I can get this app all set up for you.\r\n\r\nThanks!\r\n\r\n----\r\n{0}",
                        message));
            }

            return View(model);
        }

        public ActionResult Index() {
            return IndexViewResult(new SetupViewModel { AdminUsername = "admin" });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model) {
            //HACK: (erikpo) Couldn't get a custom ValidationAttribute to validate two properties
            if (!model.DatabaseOptions && string.IsNullOrEmpty(model.DatabaseConnectionString))
                ModelState.AddModelError("DatabaseConnectionString", "A SQL connection string is required");

            if (!ModelState.IsValid) {
                return IndexViewResult(model);
            }

            try {
                var shellSettings = new ShellSettings {
                    Name = "Default",
                    DataProvider = model.DatabaseOptions ? "SQLite" : "SqlServer",
                    DataConnectionString = model.DatabaseConnectionString
                };

                // creating a standalone environment. 
                // in theory this environment can be used to resolve any normal components by interface, and those
                // components will exist entirely in isolation - no crossover between the safemode container currently in effect
                using (var finiteEnvironment = _orchardHost.CreateStandaloneEnvironment(shellSettings)) {
                    try {
                        // initialize database before the transaction is created
                        var sessionFactoryHolder = finiteEnvironment.Resolve<ISessionFactoryHolder>();
                        sessionFactoryHolder.CreateDatabase();


                        // create superuser
                        var membershipService = finiteEnvironment.Resolve<IMembershipService>();
                        var user =
                            membershipService.CreateUser(new CreateUserParams(model.AdminUsername, model.AdminPassword,
                                                                              String.Empty, String.Empty, String.Empty,
                                                                              true));

                        
                        // set site name and settings
                        var siteService = finiteEnvironment.Resolve<ISiteService>();
                        var siteSettings = siteService.GetSiteSettings().As<SiteSettings>();
                        siteSettings.Record.SiteSalt = Guid.NewGuid().ToString("N");
                        siteSettings.Record.SiteName = model.SiteName;
                        siteSettings.Record.SuperUser = model.AdminUsername;
                        siteSettings.Record.PageTitleSeparator = " - ";

                        // set site theme
                        var themeService = finiteEnvironment.Resolve<IThemeService>();
                        themeService.SetSiteTheme("Green");

                        var contentManager = finiteEnvironment.Resolve<IContentManager>();
                         
                        // create home page as a CMS page
                        var page = contentManager.Create("page");
                        page.As<BodyAspect>().Text = "<p>Welcome to Orchard!</p><p>Congratulations, you've successfully set-up your Orchard site.</p><p>This is the home page of your new site. We've taken the liberty to write here about a few things you could look at next in order to get familiar with the application. Once you feel confident you don't need this anymore, just click [Edit] to go into edit mode and replace this with whatever you want on your home page to make it your own.</p><p>One thing you could do (but you don't have to) is go into [Manage Settings] (follow the [Admin] link and then look for it under \"Settings\" in the menu on the left) and check that everything is configured the way you want.</p><p>You probably want to make the site your own. One of the ways you can do that is by clicking [Manage Themes] in the admin menu. A theme is a packaged look and feel that affects the whole site. We have installed a few themes already, but you'll also be able to browse through an online gallery of themes created by other users of Orchard.</p><p>Next, you can start playing with the content types that we installed. For example, go ahead and click [Add New Page] in the admin menu and create an \"about\" page. Then, add it to the navigation menu by going to [Manage Navigation]. You can also click [Add New Blog] and start posting by clicking [Add New Post].</p><p>Finally, Orchard has been designed to be extended. It comes with a few built-in modules such as pages and blogs but you can install new ones by going to [Manage Themes] and clicking [Install a new Theme]. Like for themes, modules are created by other users of Orchard just like you so if you feel up to it, please [consider participating].</p><p>--The Orchard Crew</p>";
                        page.As<RoutableAspect>().Slug = "home";
                        page.As<RoutableAspect>().Title = T("Home").ToString();
                        page.As<HasComments>().CommentsShown = false;
                        page.As<CommonAspect>().Owner = user;
                        contentManager.Publish(page);
                        siteSettings.Record.HomePage = "PagesHomePageProvider;" + page.Id;

                        // add a menu item for the shiny new home page
                        var menuItem = contentManager.Create("menuitem");
                        menuItem.As<MenuPart>().MenuPosition = "1";
                        menuItem.As<MenuPart>().MenuText = T("Home").ToString();
                        menuItem.As<MenuPart>().OnMainMenu = true;
                        menuItem.As<MenuItem>().Url = "";

                        var authenticationService = finiteEnvironment.Resolve<IAuthenticationService>();
                        authenticationService.SignIn(user, true);
                         
                    }
                    catch {
                        finiteEnvironment.Resolve<ITransactionManager>().Cancel();
                        throw;
                    }
                }

                _shellSettingsLoader.SaveSettings(shellSettings);

                _orchardHost.Reinitialize();

                // redirect to the welcome page.
                return Redirect("~/");
            }
            catch (Exception exception) {
                _notifier.Error(T("Setup failed:"));
                for(var scan = exception; scan !=null; scan = scan.InnerException){
                    _notifier.Error(scan.Message);
                }
                return IndexViewResult(model);
            }
        }

        bool CanWriteToAppDataFolder(out string message) {
            try {
                _appDataFolder.CreateFile("_systemcheck.txt", "Communicator check one two one two");
                _appDataFolder.DeleteFile("_systemcheck.txt");

                message = "";
                return true;
            }
            catch (Exception ex) {
                message = ex.Message.Replace("_systemcheck.txt", "");
                return false;
            }
        }
    }
}

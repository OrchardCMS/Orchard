using System;
using System.Linq;
using System.Web;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace Orchard.Setup.Services {
    public class SetupService : ISetupService {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ICompositionStrategy _compositionStrategy;

        public SetupService(
            ShellSettings shellSettings,
            INotifier notifier,
            IOrchardHost orchardHost,
            IShellSettingsManager shellSettingsManager,
            IShellContainerFactory shellContainerFactory,
            ICompositionStrategy compositionStrategy) {
            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;
            _shellContainerFactory = shellContainerFactory;
            _compositionStrategy = compositionStrategy;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        public void Setup(SetupContext context) {
            var shellSettings = new ShellSettings(_shellSettings) {
                DataProvider = context.DatabaseProvider,
                DataConnectionString = context.DatabaseConnectionString,
                DataTablePrefix = context.DatabaseTablePrefix,
            };

            var shellDescriptor = new ShellDescriptor {
                EnabledFeatures = context.EnabledFeatures.Select(name => new ShellFeature { Name = name })
            };

            var shellToplogy = _compositionStrategy.Compose(shellSettings, shellDescriptor);

            // initialize database explicitly, and store shell descriptor
            var bootstrapLifetimeScope = _shellContainerFactory.CreateContainer(shellSettings, shellToplogy);
            using (var environment = new StandaloneEnvironment(bootstrapLifetimeScope)) {
                environment.Resolve<ISessionFactoryHolder>().CreateDatabase();

                environment.Resolve<IShellDescriptorManager>().UpdateShellDescriptor(
                    0,
                    shellDescriptor.EnabledFeatures,
                    shellDescriptor.Parameters);
            }


            // creating a standalone environment. 
            // in theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect

            // must mark state as Running - otherwise standalone enviro is created "for setup"
            shellSettings.State = new TenantState("Running");
            using (var environment = _orchardHost.CreateStandaloneEnvironment(shellSettings)) {
                try {
                    // create superuser
                    var membershipService = environment.Resolve<IMembershipService>();
                    var user =
                        membershipService.CreateUser(new CreateUserParams(context.AdminUsername, context.AdminPassword,
                                                                          String.Empty, String.Empty, String.Empty,
                                                                          true));

                    // set site name and settings
                    var siteService = environment.Resolve<ISiteService>();
                    var siteSettings = siteService.GetSiteSettings().As<SiteSettings>();
                    siteSettings.Record.SiteSalt = Guid.NewGuid().ToString("N");
                    siteSettings.Record.SiteName = context.SiteName;
                    siteSettings.Record.SuperUser = context.AdminUsername;
                    siteSettings.Record.PageTitleSeparator = " - ";

                    // set site theme
                    var themeService = environment.Resolve<IThemeService>();
                    themeService.SetSiteTheme("Classic");

                    var contentManager = environment.Resolve<IContentManager>();

                    // simulate installation-time module activation events
                    var hackInstallationGenerator = environment.Resolve<IHackInstallationGenerator>();
                    hackInstallationGenerator.GenerateInstallEvents();

                    // create home page as a CMS page
                    var page = contentManager.Create("page", VersionOptions.Draft);
                    page.As<BodyAspect>().Text = "<p>Welcome to Orchard!</p><p>Congratulations, you've successfully set-up your Orchard site.</p><p>This is the home page of your new site. We've taken the liberty to write here about a few things you could look at next in order to get familiar with the application. Once you feel confident you don't need this anymore, just click <a href=\"Admin/Pages/Edit/3\">Edit</a> to go into edit mode and replace this with whatever you want on your home page to make it your own.</p><p>One thing you could do (but you don't have to) is go into <a href=\"Admin/Settings\">Manage Settings</a> (follow the <a href=\"Admin\">Admin</a> link and then look for it under \"Settings\" in the menu on the left) and check that everything is configured the way you want.</p><p>You probably want to make the site your own. One of the ways you can do that is by clicking <a href=\"Admin/Themes\">Manage Themes</a> in the admin menu. A theme is a packaged look and feel that affects the whole site.</p><p>Next, you can start playing with the content types that we installed. For example, go ahead and click <a href=\"Admin/Pages/Create\">Add New Page</a> in the admin menu and create an \"about\" page. Then, add it to the navigation menu by going to <a href=\"Admin/Navigation\">Manage Menu</a>. You can also click <a href=\"Admin/Blogs/Create\">Add New Blog</a> and start posting by clicking \"Add New Post\".</p><p>Finally, Orchard has been designed to be extended. It comes with a few built-in modules such as pages and blogs or themes. You can install new themes by going to <a href=\"Admin/Themes\">Manage Themes</a> and clicking <a href=\"Admin/Themes/Install\">Install a new Theme</a>. Like for themes, modules are created by other users of Orchard just like you so if you feel up to it, please <a href=\"http://www.orchardproject.net/\">consider participating</a>.</p><p>--The Orchard Crew</p>";
                    page.As<RoutableAspect>().Slug = "home";
                    page.As<RoutableAspect>().Title = T("Home").ToString();
                    page.As<HasComments>().CommentsShown = false;
                    page.As<CommonAspect>().Owner = user;
                    contentManager.Publish(page);
                    siteSettings.Record.HomePage = "PageHomePageProvider;" + page.Id;

                    // add a menu item for the shiny new home page
                    var menuItem = contentManager.Create("menuitem");
                    menuItem.As<MenuPart>().MenuPosition = "1";
                    menuItem.As<MenuPart>().MenuText = T("Home").ToString();
                    menuItem.As<MenuPart>().OnMainMenu = true;
                    menuItem.As<MenuItem>().Url = "";

                    //Temporary fix for running setup on command line
                    if (HttpContext.Current != null) {
                        var authenticationService = environment.Resolve<IAuthenticationService>();
                        authenticationService.SignIn(user, true);
                    }
                }
                catch {
                    environment.Resolve<ITransactionManager>().Cancel();
                    throw;
                }
            }

            _shellSettingsManager.SaveSettings(shellSettings);
        }
    }
}
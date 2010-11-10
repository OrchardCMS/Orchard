using System;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Routable.Models;
using Orchard.Core.Settings.Descriptor.Records;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Reports.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.Environment.State;
using Orchard.Data.Migration;
using Orchard.Themes.Services;
using Orchard.Widgets.Models;
using Orchard.Widgets;

namespace Orchard.Setup.Services {
    public class SetupService : ISetupService {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IProcessingEngine _processingEngine;

        public SetupService(
            ShellSettings shellSettings,
            IOrchardHost orchardHost,
            IShellSettingsManager shellSettingsManager,
            IShellContainerFactory shellContainerFactory,
            ICompositionStrategy compositionStrategy,
            IProcessingEngine processingEngine) {
            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;
            _shellContainerFactory = shellContainerFactory;
            _compositionStrategy = compositionStrategy;
            _processingEngine = processingEngine;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ShellSettings Prime() {
            return _shellSettings;
        }

        public void Setup(SetupContext context) {
            // The vanilla Orchard distibution has the following features enabled.
            if (context.EnabledFeatures == null || context.EnabledFeatures.Count() == 0) {
                string[] hardcoded = {
                    "Orchard.Framework",
                    "Common",
                    "Shapes",
                    "Contents",
                    "Dashboard",
                    "Reports",
                    "Feeds",
                    "HomePage",
                    "Navigation",
                    "Scheduling",
                    "Indexing",
                    "Localization",
                    "Routable",
                    "Settings",
                    "XmlRpc",
                    "Messaging",
                    "Orchard.Users",
                    "Orchard.Roles",
                    "TinyMce",
                    "PackagingServices",
                    "Orchard.Modules",
                    "Orchard.Themes",
                    "Orchard.PublishLater",
                    "Orchard.Blogs",
                    "Orchard.Comments",
                    "Orchard.Tags",
                    "Orchard.Media",
                    "Orchard.Widgets",
                    "Orchard.jQuery",
                    "TheThemeMachine",
                };

                context.EnabledFeatures = hardcoded;
            }

            
            var shellSettings = new ShellSettings(_shellSettings);

            if (string.IsNullOrEmpty(shellSettings.DataProvider)) {
                shellSettings.DataProvider = context.DatabaseProvider;
                shellSettings.DataConnectionString = context.DatabaseConnectionString;
                shellSettings.DataTablePrefix = context.DatabaseTablePrefix;
            }

            var shellDescriptor = new ShellDescriptor {
                Features = context.EnabledFeatures.Select(name => new ShellFeature { Name = name })
            };

            var shellBlueprint = _compositionStrategy.Compose(shellSettings, shellDescriptor);

            // initialize database explicitly, and store shell descriptor
            var bootstrapLifetimeScope = _shellContainerFactory.CreateContainer(shellSettings, shellBlueprint);

            using (var environment = bootstrapLifetimeScope.CreateWorkContextScope()) {

                // check if the database is already created (in case an exception occured in the second phase)
                var shellDescriptorRepository = environment.Resolve<IRepository<ShellDescriptorRecord>>();
                try {
                    shellDescriptorRepository.Get(x => true);
                }
                catch {
                    var schemaBuilder = new SchemaBuilder(environment.Resolve<IDataMigrationInterpreter>());
                    var reportsCoordinator = environment.Resolve<IReportsCoordinator>();

                    reportsCoordinator.Register("Data Migration", "Setup", "Orchard installation");

                    schemaBuilder.CreateTable("Orchard_Framework_DataMigrationRecord",
                                              table => table
                                                           .Column<int>("Id", column => column.PrimaryKey().Identity())
                                                           .Column<string>("DataMigrationClass")
                                                           .Column<int>("Version"));

                    var dataMigrationManager = environment.Resolve<IDataMigrationManager>();
                    dataMigrationManager.Update("Settings");

                    foreach ( var feature in context.EnabledFeatures ) {
                        dataMigrationManager.Update(feature);
                    }

                    environment.Resolve<IShellDescriptorManager>().UpdateShellDescriptor(
                        0,
                        shellDescriptor.Features,
                        shellDescriptor.Parameters);
                }
            }

            // in effect "pump messages" see PostMessage circa 1980
            while ( _processingEngine.AreTasksPending() )
                _processingEngine.ExecuteNextTask();


            // creating a standalone environment. 
            // in theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect

            // must mark state as Running - otherwise standalone enviro is created "for setup"
            shellSettings.State = new TenantState("Running");
            using (var environment = _orchardHost.CreateStandaloneEnvironment(shellSettings)) {
                try {
                    CreateTenantData(context, environment);
                }
                catch {
                    environment.Resolve<ITransactionManager>().Cancel();
                    throw;
                }
            }

            _shellSettingsManager.SaveSettings(shellSettings);
        }

        private void CreateTenantData(SetupContext context, IWorkContextScope environment) {
            // create superuser
            var membershipService = environment.Resolve<IMembershipService>();
            var user =
                membershipService.CreateUser(new CreateUserParams(context.AdminUsername, context.AdminPassword,
                                                                  String.Empty, String.Empty, String.Empty,
                                                                  true));

            // set superuser as current user for request (it will be set as the owner of all content items)
            var authenticationService = environment.Resolve<IAuthenticationService>();
            authenticationService.SetAuthenticatedUserForRequest(user);

            // set site name and settings
            var siteService = environment.Resolve<ISiteService>();
            var siteSettings = siteService.GetSiteSettings().As<SiteSettingsPart>();
            siteSettings.Record.SiteSalt = Guid.NewGuid().ToString("N");
            siteSettings.Record.SiteName = context.SiteName;
            siteSettings.Record.SuperUser = context.AdminUsername;
            siteSettings.Record.PageTitleSeparator = " - ";
            siteSettings.Record.SiteCulture = "en-US";

            // set site theme
            var themeService = environment.Resolve<ISiteThemeService>();
            themeService.SetSiteTheme("TheThemeMachine");

            // add default culture
            var cultureManager = environment.Resolve<ICultureManager>();
            cultureManager.AddCulture("en-US");

            var contentManager = environment.Resolve<IContentManager>();

            // this needs to exit the standalone environment? rework this process entirely?
            // simulate installation-time module activation events
            //var hackInstallationGenerator = environment.Resolve<IHackInstallationGenerator>();
            //hackInstallationGenerator.GenerateInstallEvents();

            var contentDefinitionManager = environment.Resolve<IContentDefinitionManager>();
            //todo: (heskew) pull these definitions (and initial content creation) out into more appropriate modules
            contentDefinitionManager.AlterTypeDefinition("BlogPost", cfg => cfg
                .WithPart("CommentsPart")
                .WithPart("TagsPart")
                .WithPart("LocalizationPart")
                .Draftable()
                .Indexed()
                );
            contentDefinitionManager.AlterTypeDefinition("Page", cfg => cfg
                .WithPart("CommonPart")
                .WithPart("PublishLaterPart")
                .WithPart("RoutePart")
                .WithPart("BodyPart")
                .WithPart("TagsPart")
                .WithPart("LocalizationPart")
                .Creatable()
                .Draftable()
                .Indexed()
                );
            contentDefinitionManager.AlterPartDefinition("BodyPart", cfg => cfg
                .WithSetting("BodyPartSettings.FlavorDefault", BodyPartSettings.FlavorDefaultDefault));

            // If "Orchard.Widgets" is enabled, setup default layers and widgets
            var extensionManager = environment.Resolve<IExtensionManager>();
            var shellDescriptor = environment.Resolve<ShellDescriptor>();
            if (extensionManager.EnabledFeatures(shellDescriptor).Where(d => d.Name == "Orchard.Widgets").Any()) {
                // Create default layers
                var layerInitializer = environment.Resolve<IDefaultLayersInitializer>();
                layerInitializer.CreateDefaultLayers();

                // add a layer for the homepage
                var homepageLayer = contentManager.Create("Layer");
                homepageLayer.As<LayerPart>().Name = "TheHomepage";
                homepageLayer.As<LayerPart>().LayerRule = "url \"~/\"";
                contentManager.Publish(homepageLayer);

                // and three more for the tripel...really need this elsewhere...
                var tripelFirst = contentManager.Create("HtmlWidget");
                tripelFirst.As<WidgetPart>().LayerPart = homepageLayer.As<LayerPart>();
                tripelFirst.As<WidgetPart>().Title = T("First Leader Aside").Text;
                tripelFirst.As<WidgetPart>().Zone = "TripelFirst";
                tripelFirst.As<WidgetPart>().Position = "5";
                tripelFirst.As<BodyPart>().Text = "<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur a nibh ut tortor dapibus vestibulum. Aliquam vel sem nibh. Suspendisse vel condimentum tellus.</p>";
                contentManager.Publish(tripelFirst);

                var tripelSecond = contentManager.Create("HtmlWidget");
                tripelSecond.As<WidgetPart>().LayerPart = homepageLayer.As<LayerPart>();
                tripelSecond.As<WidgetPart>().Title = T("Second Leader Aside").Text;
                tripelSecond.As<WidgetPart>().Zone = "TripelSecond";
                tripelSecond.As<WidgetPart>().Position = "5";
                tripelSecond.As<BodyPart>().Text = "<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur a nibh ut tortor dapibus vestibulum. Aliquam vel sem nibh. Suspendisse vel condimentum tellus.</p>";
                contentManager.Publish(tripelSecond);

                var tripelThird = contentManager.Create("HtmlWidget");
                tripelThird.As<WidgetPart>().LayerPart = homepageLayer.As<LayerPart>();
                tripelThird.As<WidgetPart>().Title = T("Third Leader Aside").Text;
                tripelThird.As<WidgetPart>().Zone = "TripelThird";
                tripelThird.As<WidgetPart>().Position = "5";
                tripelThird.As<BodyPart>().Text = "<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur a nibh ut tortor dapibus vestibulum. Aliquam vel sem nibh. Suspendisse vel condimentum tellus.</p>";
                contentManager.Publish(tripelThird);
            }

            // create a welcome page that's promoted to the home page
            var page = contentManager.Create("Page");
            page.As<RoutePart>().Title = T("Welcome to Orchard!").Text;
            page.As<BodyPart>().Text = "<p>You’ve successfully setup your Orchard Site and this is the homepage of your new site. Here are a few things you can look at to get familiar with the application. Once you feel confident you don’t need this anymore, you can <a href=\"/Admin/Contents/Edit/7\">remove this by going into editing mode</a> and replacing it with whatever you want.</p><p>First things first - You’ll probably want to <a href=\"Admin/Settings\">manage your settings</a> and configure Orchard to your liking. After that, you can head over to <a href=\"Admin/Themes\">manage themes to change or install new themes</a> and really make it your own. Once you’re happy with a look and feel, it’s time for some content. You can start creating new custom content types or start with some built-in ones by <a href=\"Admin/Pages/Create\">adding a page</a>, <a href=\"Admin/Blogs/Create\">creating a blog</a> or <a href=\"Admin/Navigation\">managing your menus.</a></p><p>Finally, Orchard has been designed to be extended. It comes with a few built-in modules such as pages and blogs or themes. If you’re looking to add additional functionality, you can do so by creating your own module or installing a new one that someone has made. Modules are created by other users of Orchard just like you so if you feel up to it, <a href=\"http://www.orchardproject.net/\">please consider participating</a>. XOXO – The Orchard Team </p>";

            contentManager.Publish(page);
            siteSettings.Record.HomePage = "RoutableHomePageProvider;" + page.Id;

            // add a menu item for the shiny new home page
            var menuItem = contentManager.Create("MenuItem");
            menuItem.As<MenuPart>().MenuPosition = "1";
            menuItem.As<MenuPart>().MenuText = T("Home").ToString();
            menuItem.As<MenuPart>().OnMainMenu = true;
            menuItem.As<MenuItemPart>().Url = "";

            //null check: temporary fix for running setup in command line
            if (HttpContext.Current != null) {
                authenticationService.SignIn(user, true);
            }
        }
    }
}
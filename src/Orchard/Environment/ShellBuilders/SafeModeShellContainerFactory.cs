using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Migrations;
using Orchard.Environment.Configuration;
using Orchard.Extensions;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ModelBinders;
using Orchard.Mvc.Routes;
using Orchard.Mvc.ViewEngines;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.UI.PageClass;
using Orchard.UI.PageTitle;
using Orchard.UI.Zones;

namespace Orchard.Environment.ShellBuilders {
    public class SafeModeShellContainerFactory : IShellContainerFactory {
        private readonly IContainer _container;

        public SafeModeShellContainerFactory(IContainer container) {
            _container = container;
        }

        public IContainer CreateContainer(IShellSettings settings) {
            // when you have settings the setup container factory is not in effect
            if (settings != null) {
                return null;
            }

            var shellContainer = _container.CreateInnerContainer();

            shellContainer.Build(builder => {
                // standard services needed in safe mode
                builder.Register<DefaultOrchardShell>().As<IOrchardShell>().ContainerScoped();
                builder.Register<RoutePublisher>().As<IRoutePublisher>().ContainerScoped();
                builder.Register<ModelBinderPublisher>().As<IModelBinderPublisher>().ContainerScoped();
                builder.Register<MvcModule>().As<IModule>().ContainerScoped();
                builder.Register<WebFormsViewEngineProvider>().As<IViewEngineProvider>().ContainerScoped();
                builder.Register<ViewEngineFilter>().As<IFilterProvider>().ContainerScoped();
                builder.Register<PageTitleBuilder>().As<IPageTitleBuilder>().ContainerScoped();
                builder.Register<ZoneManager>().As<IZoneManager>().ContainerScoped();
                builder.Register<PageClassBuilder>().As<IPageClassBuilder>().ContainerScoped();
                builder.Register<Notifier>().As<INotifier>().ContainerScoped();
                builder.Register<NotifyFilter>().As<IFilterProvider>().ContainerScoped();
                builder.Register<DatabaseMigrationManager>().As<IDatabaseMigrationManager>().ContainerScoped();

                // safe mode specific implementations of needed service interfaces
                builder.Register<NullHackInstallationGenerator>().As<IHackInstallationGenerator>().ContainerScoped();
                builder.Register<SafeModeThemeService>().As<IThemeService>().ContainerScoped();
                builder.Register<SafeModeText>().As<IText>().ContainerScoped();
                builder.Register<SafeModeSiteService>().As<ISiteService>().ContainerScoped();


                //review: discovery an registration from limited number of modules (e.g. setup) should also be done here
                //review: this should come from the limited number of added modules
                builder.Register<SetupRouteProvider>().As<IRouteProvider>().ContainerScoped();
            });

            shellContainer.Build(builder => {
                foreach (var module in shellContainer.Resolve<IEnumerable<IModule>>()) {
                    builder.RegisterModule(module);
                }
            });

            return shellContainer;
        }

        //review: this should come from the subset of modules, not this factory
        class SetupRouteProvider : IRouteProvider {
            public IEnumerable<RouteDescriptor> GetRoutes() {
                var routes = new List<RouteDescriptor>();
                GetRoutes(routes);
                return routes;
            }

            public void GetRoutes(ICollection<RouteDescriptor> routes) {
                routes.Add(new RouteDescriptor {
                    Priority = 100,
                    Route = new Route("{controller}/{action}",
                        new RouteValueDictionary { { "Area", "Orchard.Setup" }, { "Controller", "Setup" }, { "Action", "Index" } },
                        new RouteValueDictionary { { "Area", "Orchard.Setup" }, { "Controller", "Setup" }, { "Action", "Index" } },
                        new RouteValueDictionary { { "Area", "Orchard.Setup" } },
                        new MvcRouteHandler())
                });
            }
        }

        class SafeModeText : IText {
            public LocalizedString Get(string textHint, params object[] args) {
                if (args == null || args.Length == 0) {
                    return new LocalizedString(textHint);
                }
                return new LocalizedString(string.Format(textHint, args));
            }
        }

        class SafeModeThemeService : IThemeService {
            class SafeModeTheme : ITheme {
                public ContentItem ContentItem { get; set; }
                public string ThemeName { get; set; }
                public string DisplayName { get; set; }
                public string Description { get; set; }
                public string Version { get; set; }
                public string Author { get; set; }
                public string HomePage { get; set; }
            }

            private readonly SafeModeTheme _theme = new SafeModeTheme {
                ThemeName = "SafeMode",
                DisplayName = "SafeMode",
            };

            public ITheme GetThemeByName(string themeName) { return _theme; }
            public ITheme GetSiteTheme() { return _theme; }
            public void SetSiteTheme(string themeName) { }
            public ITheme GetRequestTheme(RequestContext requestContext) { return _theme; }
            public IEnumerable<ITheme> GetInstalledThemes() { return new[] { _theme }; }
            public void InstallTheme(HttpPostedFileBase file) { }
            public void UninstallTheme(string themeName) { }
        }

        class NullHackInstallationGenerator : IHackInstallationGenerator {
            public void GenerateInstallEvents() { }
            public void GenerateActivateEvents() { }
        }

        class SafeModeSiteService : ISiteService {
            public ISite GetSiteSettings() {
                var site = new ContentItemBuilder("site")
                    .Weld<SafeModeSite>()
                    .Build();

                return site.As<ISite>();
            }
        }

        class SafeModeSite : ContentPart, ISite {
            public string PageTitleSeparator {
                get { return "*"; }
            }

            public string SiteName {
                get { return "Orchard Setup"; }
            }

            public string SiteSalt {
                get { return "42"; }
            }

            public string SiteUrl {
                get { return "/"; }
            }

            public string SuperUser {
                get { return ""; }
            }
        }
    }

}

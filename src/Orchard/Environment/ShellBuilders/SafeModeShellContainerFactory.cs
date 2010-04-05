using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Builders;
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

        public ILifetimeScope CreateContainer(IShellSettings settings) {
            // when you have settings the setup container factory is not in effect
            if (settings != null) {
                return null;
            }

            var shellScope = _container.BeginLifetimeScope();
            var builder = new ContainerUpdater();
            // standard services needed in safe mode
            builder.RegisterType<DefaultOrchardShell>().As<IOrchardShell>().InstancePerLifetimeScope();
            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().InstancePerLifetimeScope();
            builder.RegisterType<ModelBinderPublisher>().As<IModelBinderPublisher>().InstancePerLifetimeScope();
            builder.RegisterType<MvcModule>().As<IModule>().InstancePerLifetimeScope();
            builder.RegisterType<WebFormsViewEngineProvider>().As<IViewEngineProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ViewEngineFilter>().As<IFilterProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ThemeFilter>().As<IFilterProvider>().InstancePerLifetimeScope();
            builder.RegisterType<PageTitleBuilder>().As<IPageTitleBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<ZoneManager>().As<IZoneManager>().InstancePerLifetimeScope();
            builder.RegisterType<PageClassBuilder>().As<IPageClassBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<Notifier>().As<INotifier>().InstancePerLifetimeScope();
            builder.RegisterType<NotifyFilter>().As<IFilterProvider>().InstancePerLifetimeScope();
            builder.RegisterType<SessionFactoryBuilder>().As<ISessionFactoryBuilder>().InstancePerLifetimeScope();
            // safe mode specific implementations of needed service interfaces
            builder.RegisterType<NullHackInstallationGenerator>().As<IHackInstallationGenerator>().InstancePerLifetimeScope();
            builder.RegisterType<SafeModeThemeService>().As<IThemeService>().InstancePerLifetimeScope();
            builder.RegisterType<SafeModeText>().As<IText>().InstancePerLifetimeScope();
            builder.RegisterType<SafeModeSiteService>().As<ISiteService>().InstancePerLifetimeScope();
            // yes, this is brutal, and if you are reading this, I sincerely apologize.
            var dependencies = Assembly.Load("Orchard.Setup")
                .GetExportedTypes()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(IDependency).IsAssignableFrom(type));
            foreach (var serviceType in dependencies) {
                foreach (var interfaceType in serviceType.GetInterfaces()) {
                    if (typeof(IDependency).IsAssignableFrom(interfaceType)) {
                        var registrar = builder.RegisterType(serviceType).As(interfaceType);
                        if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType)) {
                            registrar.SingleInstance();
                        }
                        else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType)) {
                            registrar.InstancePerDependency();
                        }
                        else {
                            registrar.InstancePerLifetimeScope();
                        }
                    }
                }
            }
            builder.Update(shellScope);
            var modulesUpdater = new ContainerUpdater();
            foreach (var module in _container.Resolve<IEnumerable<IModule>>()) {
                modulesUpdater.RegisterModule(module);
            }
            modulesUpdater.Update(shellScope);

            return shellScope;
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
                public string Tags { get; set; }
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

            public string HomePage {
                get { return ""; }
                set { throw new NotImplementedException(); }
            }
        }
    }

}

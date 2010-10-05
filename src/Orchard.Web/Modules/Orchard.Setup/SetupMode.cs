using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Autofac;
using Orchard.Commands;
using Orchard.Commands.Builtin;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Providers;
using Orchard.Data.Migration;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.ModelBinders;
using Orchard.Mvc.Routes;
using Orchard.Mvc.ViewEngines;
using Orchard.Mvc.ViewEngines.Razor;
using Orchard.Mvc.ViewEngines.ThemeAwareness;
using Orchard.Mvc.ViewEngines.WebForms;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.UI.PageClass;
using Orchard.UI.PageTitle;
using Orchard.UI.Resources;
using Orchard.UI.Zones;
using IFilterProvider = Orchard.Mvc.Filters.IFilterProvider;
using ResourceManifest = Orchard.Core.Shapes.ResourceManifest;

namespace Orchard.Setup {
    public class SetupMode : Module {
        public Feature Feature { get; set; }

        protected override void Load(ContainerBuilder builder) {

            // standard services needed in setup mode
            builder.RegisterModule(new MvcModule());
            builder.RegisterModule(new CommandModule());
            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().InstancePerLifetimeScope();
            builder.RegisterType<ModelBinderPublisher>().As<IModelBinderPublisher>().InstancePerLifetimeScope();
            builder.RegisterType<WebFormViewEngineProvider>().As<IViewEngineProvider>().As<IShapeTemplateViewEngine>().InstancePerLifetimeScope();
            builder.RegisterType<RazorViewEngineProvider>().As<IViewEngineProvider>().As<IShapeTemplateViewEngine>().InstancePerLifetimeScope();
            builder.RegisterType<ThemedViewResultFilter>().As<IFilterProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ThemeFilter>().As<IFilterProvider>().InstancePerLifetimeScope();
            builder.RegisterType<PageTitleBuilder>().As<IPageTitleBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<PageClassBuilder>().As<IPageClassBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<Notifier>().As<INotifier>().InstancePerLifetimeScope();
            builder.RegisterType<NotifyFilter>().As<IFilterProvider>().InstancePerLifetimeScope();
            builder.RegisterType<DataServicesProviderFactory>().As<IDataServicesProviderFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DefaultCommandManager>().As<ICommandManager>().InstancePerLifetimeScope();
            builder.RegisterType<HelpCommand>().As<ICommandHandler>().InstancePerLifetimeScope();
            builder.RegisterType<DefaultWorkContextAccessor>().As<IWorkContextAccessor>().InstancePerMatchingLifetimeScope("shell");
            builder.RegisterType<ResourceManifest>().As<IResourceManifestProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ResourceManager>().As<IResourceManager>().InstancePerLifetimeScope();
            builder.RegisterType<ResourceFilter>().As<IFilterProvider>().InstancePerLifetimeScope();

            // setup mode specific implementations of needed service interfaces
            builder.RegisterType<SafeModeThemeService>().As<IThemeService>().InstancePerLifetimeScope();
            builder.RegisterType<SafeModeText>().As<IText>().InstancePerLifetimeScope();
            builder.RegisterType<SafeModeSiteService>().As<ISiteService>().InstancePerLifetimeScope();

            builder.RegisterType<DefaultDataMigrationInterpreter>().As<IDataMigrationInterpreter>().InstancePerLifetimeScope();
            builder.RegisterType<DataMigrationManager>().As<IDataMigrationManager>().InstancePerLifetimeScope();

            // in progress - adding services for display/shape support in setup
            builder.RegisterType<DisplayHelperFactory>().As<IDisplayHelperFactory>();
            builder.RegisterType<DefaultDisplayManager>().As<IDisplayManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<ShapeHelperFactory>().As<IShapeHelperFactory>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();

            builder.RegisterType<ThemeAwareViewEngine>().As<IThemeAwareViewEngine>();
            builder.RegisterType<LayoutAwareViewEngine>().As<ILayoutAwareViewEngine>();
            builder.RegisterType<ConfiguredEnginesCache>().As<IConfiguredEnginesCache>();
            builder.RegisterType<PageWorkContext>().As<IWorkContextStateProvider>();
            builder.RegisterType<SafeModeSiteWorkContextProvider>().As<IWorkContextStateProvider>();

            builder.RegisterType<ShapeTemplateBindingStrategy>().As<IShapeTableProvider>();
            builder.RegisterType<BasicShapeTemplateHarvester>().As<IShapeTemplateHarvester>();
            builder.RegisterType<ShapeAttributeBindingStrategy>().As<IShapeTableProvider>();
            builder.RegisterModule(new ShapeAttributeBindingModule());
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
                public string Zones { get; set; }
                public string BaseTheme { get; set; }
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

        class SafeModeSiteWorkContextProvider : IWorkContextStateProvider {
            public T Get<T>(string name) {
                if (name == "CurrentSite")
                    return (T)(ISite) new SafeModeSite();
                return default(T);
            }
        }

        class SafeModeSiteService : ISiteService {
            public ISite GetSiteSettings() {
                var siteType = new ContentTypeDefinitionBuilder().Named("Site").Build();
                var site = new ContentItemBuilder(siteType)
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

            public string SiteCulture {
                get { return ""; }
                set { throw new NotImplementedException(); }
            }

            public ResourceDebugMode ResourceDebugMode {
                get { return ResourceDebugMode.FromAppSetting;  }
                set { throw new NotImplementedException(); }
            }
        }
    }
}

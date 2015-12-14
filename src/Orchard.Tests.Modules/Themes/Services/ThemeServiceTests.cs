using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Autofac;
using Moq;
using NHibernate;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.ContentManagement.Records;
using Orchard.Core.Settings.Descriptor.Records;
using Orchard.Core.Settings.Metadata;
using Orchard.Core.Settings.Models;
using Orchard.Core.Settings.Services;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Environment.AutofacUtil.DynamicProxy2;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Modules;
using Orchard.Modules.Services;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Settings;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;
using Orchard.Themes;
using Orchard.Themes.Handlers;
using Orchard.Themes.Models;
using Orchard.Themes.Services;
using Orchard.UI.Notify;

namespace Orchard.Tests.Modules.Themes.Services {
#if REFACTORING
    [TestFixture, Ignore]
    public class ThemeServiceTests {
        private IThemeService _themeService;
        private ISiteThemeService _siteThemeService;
        private IContainer _container;
        private ISessionFactory _sessionFactory;
        private ISession _session;
        private IFeatureManager _featureManager;

        [TestFixtureSetUp]
        public void InitFixture() {
            var databaseFileName = System.IO.Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(databaseFileName,
                typeof(ThemeSiteSettingsPartRecord),
                typeof(SiteSettingsPartRecord),
                typeof(ContentItemVersionRecord),
                typeof(ContentItemRecord),
                typeof(ContentTypeRecord));
        }

        [TestFixtureTearDown]
        public void TermFixture() { }

        [SetUp]
        public void Init() {
            var context = new DynamicProxyContext();
            var builder = new ContainerBuilder();
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<ThemeService>().EnableDynamicProxy(context).As<IThemeService>();
            builder.RegisterType<SiteService>().As<ISiteService>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            builder.RegisterType<Orchard.Localization.Text>().As<IText>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<ContentDefinitionManager>().As<IContentDefinitionManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();
            builder.RegisterType<SiteSettingsPartHandler>().As<IContentHandler>();
            builder.RegisterType<ThemeSiteSettingsPartHandler>().As<IContentHandler>();
            //builder.RegisterType<ModuleService>().As<IModuleService>();
            builder.RegisterType<ShellDescriptor>();
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterType<StubShellDescriptorManager>().As<IShellDescriptorManager>().InstancePerLifetimeScope();
            builder.RegisterType<TransactionManager>().As<ITransactionManager>();
            builder.RegisterType<Notifier>().As<INotifier>();
            builder.RegisterType<StubAuthorizer>().As<IAuthorizer>();
            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();
            builder.RegisterType(typeof(SettingsFormatter))
                .As(typeof(IMapper<XElement, SettingsDictionary>))
                .As(typeof(IMapper<SettingsDictionary, XElement>));
            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new TestSessionLocator(_session)).As<ISessionLocator>();
            builder.RegisterAutoMocking(MockBehavior.Loose);
            _container = builder.Build();
            _themeService = _container.Resolve<IThemeService>();
            _siteThemeService = _container.Resolve<ISiteThemeService>();
            _featureManager = _container.Resolve<IFeatureManager>();
        }

        //todo: test theme feature enablement

        [Test]
        public void ThemeWithNoBaseThemeCanBeSetAsSiteTheme() {
            _siteThemeService.SetSiteTheme("ThemeOne");
            var siteTheme = _siteThemeService.GetSiteTheme();
            Assert.That(siteTheme.Name, Is.EqualTo("ThemeOne"));
        }

        [Test]
        public void ThemeWithAvailableBaseThemeCanBeSetAsSiteTheme() {
            _siteThemeService.SetSiteTheme("ThemeTwo");
            var siteTheme = _siteThemeService.GetSiteTheme();
            Assert.That(siteTheme.Name, Is.EqualTo("ThemeTwo"));
            Assert.That(siteTheme.BaseTheme, Is.EqualTo("ThemeOne"));
        }

        [Test]
        public void ThemeWithUnavailableBaseThemeCanBeSetAsSiteTheme() {
            _siteThemeService.SetSiteTheme("ThemeOne");
            _siteThemeService.SetSiteTheme("ThemeThree");
            var siteTheme = _siteThemeService.GetSiteTheme();
            Assert.That(siteTheme.Name, Is.EqualTo("ThemeOne"));
        }

        [Test]
        public void ThemeWithCircularBaseDepTrowsExceptionOnActivation() {
            _siteThemeService.SetSiteTheme("ThemeOne");
            try {
                _siteThemeService.SetSiteTheme("ThemeFourBasedOnFive");
            } catch (InvalidOperationException ex) {
                Assert.That(ex.Message, Is.StringMatching("ThemeFiveBasedOnFour"));
            }
            var siteTheme = _siteThemeService.GetSiteTheme();
            Assert.That(siteTheme.Name, Is.EqualTo("ThemeOne"));
        }

        [Test]
        public void CanEnableAndDisableThemes() {
            _featureManager.EnableFeature("ThemeOne");
            Assert.IsTrue(_themeService.GetThemeByName("ThemeOne").Enabled);
            Assert.IsTrue(_container.Resolve<IShellDescriptorManager>().GetShellDescriptor().Features.Any(sf => sf.Name == "ThemeOne"));
            _featureManager.DisableFeature("ThemeOne");
            Assert.IsFalse(_themeService.GetThemeByName("ThemeOne").Enabled);
            Assert.IsFalse(_container.Resolve<IShellDescriptorManager>().GetShellDescriptor().Features.Any(sf => sf.Name == "ThemeOne"));
        }

        [Test]
        public void ActivatingThemeEnablesIt() {
            _siteThemeService.SetSiteTheme("ThemeOne");
            Assert.IsTrue(_themeService.GetThemeByName("ThemeOne").Enabled);
            Assert.IsTrue(_container.Resolve<IShellDescriptorManager>().GetShellDescriptor().Features.Any(sf => sf.Name == "ThemeOne"));
        }

        [Test]
        public void ActivatingThemeDoesNotDisableOldTheme() {
            _siteThemeService.SetSiteTheme("ThemeOne");
            _siteThemeService.SetSiteTheme("ThemeTwo");
            Assert.IsTrue(_themeService.GetThemeByName("ThemeOne").Enabled);
            Assert.IsTrue(_themeService.GetThemeByName("ThemeTwo").Enabled);
            Assert.IsTrue(_container.Resolve<IShellDescriptorManager>().GetShellDescriptor().Features.Any(sf => sf.Name == "ThemeOne"));
            Assert.IsTrue(_container.Resolve<IShellDescriptorManager>().GetShellDescriptor().Features.Any(sf => sf.Name == "ThemeTwo"));
        }
        

        #region Stubs

        public class TestSessionLocator : ISessionLocator {
            private readonly ISession _session;

            public TestSessionLocator(ISession session) {
                _session = session;
            }

            public ISession For(Type entityType) {
                return _session;
            }
        }

        public class StubAuthorizer : IAuthorizer {
            public bool Authorize(Permission permission) {
                return true;
            }
            public bool Authorize(Permission permission, LocalizedString message) {
                return true;
            }
            public bool Authorize(Permission permission, IContent content, LocalizedString message) {
                return true;
            }
        }

        public class StubExtensionManager : IExtensionManager {
            public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
                var extensions = new[] {
                    new ExtensionDescriptor {Name = "ThemeOne", ExtensionType = "Theme"},
                    new ExtensionDescriptor {Name = "ThemeTwo", BaseTheme = "ThemeOne", ExtensionType = "Theme"},
                    new ExtensionDescriptor {Name = "ThemeThree", BaseTheme = "TheThemeThatIsntThere", ExtensionType = "Theme"},
                    new ExtensionDescriptor {Name = "ThemeFourBasedOnFive", BaseTheme = "ThemeFiveBasedOnFour", ExtensionType = "Theme"},
                    new ExtensionDescriptor {Name = "ThemeFiveBasedOnFour", BaseTheme = "ThemeFourBasedOnFive", ExtensionType = "Theme"},
                };

                foreach (var extension in extensions) {
                    extension.Features = new[] { new FeatureDescriptor { Extension = extension, Name = extension.Name } };
                    yield return extension;
                }
            }

            public IEnumerable<FeatureDescriptor> AvailableFeatures() {
                return AvailableExtensions().SelectMany(ed => ed.Features);
            }

            public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
                return featureDescriptors.Select(FrameworkFeature);
            }

            private static Feature FrameworkFeature(FeatureDescriptor descriptor) {
                return new Feature {
                    Descriptor = descriptor
                };
            }

            public void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle) {
                throw new NotImplementedException();
            }

            public void UninstallExtension(string extensionType, string extensionName) {
                throw new NotImplementedException();
            }

            public void Monitor(Action<IVolatileToken> monitor) {
                throw new NotImplementedException();
            }
        }

        public class StubShellDescriptorManager : IShellDescriptorManager {
            private readonly ShellDescriptorRecord _shellDescriptorRecord = new ShellDescriptorRecord();
            public ShellDescriptor GetShellDescriptor() {
                return GetShellDescriptorFromRecord(_shellDescriptorRecord);
            }

            public void UpdateShellDescriptor(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters) {
                _shellDescriptorRecord.Features.Clear();
                foreach (var feature in enabledFeatures) {
                    _shellDescriptorRecord.Features.Add(new ShellFeatureRecord { Name = feature.Name, ShellDescriptorRecord = null });
                }

                _shellDescriptorRecord.Parameters.Clear();
                foreach (var parameter in parameters) {
                    _shellDescriptorRecord.Parameters.Add(new ShellParameterRecord {
                        Component = parameter.Component,
                        Name = parameter.Name,
                        Value = parameter.Value,
                        ShellDescriptorRecord = null
                    });
                }
            }

            private static ShellDescriptor GetShellDescriptorFromRecord(ShellDescriptorRecord shellDescriptorRecord) {
                return new ShellDescriptor {
                    SerialNumber = shellDescriptorRecord.SerialNumber,
                    Features = shellDescriptorRecord.Features.Select(descriptorFeatureRecord => new ShellFeature {Name = descriptorFeatureRecord.Name}).ToList(),
                    Parameters = shellDescriptorRecord.Parameters.Select(descriptorParameterRecord => new ShellParameter {
                        Component = descriptorParameterRecord.Component, Name = descriptorParameterRecord.Name, Value = descriptorParameterRecord.Value
                    }).ToList()
                };
            }
        }

        #endregion
    }
#endif
}

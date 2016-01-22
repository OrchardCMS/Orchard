using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Xml.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Core.Settings.Descriptor;
using Orchard.Core.Settings.Descriptor.Records;
using Orchard.Core.Settings.State;
using Orchard.Data.Migration;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Environment.State;
using Orchard.Events;
using Orchard.FileSystems.VirtualPath;
using Orchard.Packaging.GalleryServer;
using Orchard.Packaging.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.RecipeHandlers;
using Orchard.Tests.DisplayManagement.Descriptors;
using Orchard.Tests.Environment.Extensions;
using Orchard.Tests.Environment.Features;
using Orchard.Tests.Stubs;
using Orchard.Tests.UI.Navigation;
using Orchard.Themes.Services;
using IPackageManager = Orchard.Packaging.Services.IPackageManager;

namespace Orchard.Tests.Modules.Recipes.RecipeHandlers {
    [TestFixture]
    public class ThemeRecipeHandlerTest : DatabaseEnabledTestsBase {
        private ExtensionManagerTests.StubFolders _folders;
        private ModuleRecipeHandlerTest.StubPackagingSourceManager _packagesInRepository;
        private ModuleRecipeHandlerTest.StubPackageManager _packageManager;

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                    typeof (ShellDescriptorRecord),
                    typeof (ShellFeatureRecord),
                    typeof (ShellParameterRecord),
                };
            }
        }

        public override void Register(ContainerBuilder builder) {
            var _testVirtualPathProvider = new StylesheetBindingStrategyTests.TestVirtualPathProvider();

            builder.RegisterInstance(new ShellSettings { Name = "Default" });

            _folders = new ExtensionManagerTests.StubFolders();
            _packagesInRepository = new ModuleRecipeHandlerTest.StubPackagingSourceManager();
            _packageManager = new ModuleRecipeHandlerTest.StubPackageManager();
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<FeatureManager>().As<IFeatureManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();
            builder.RegisterType<ShellDescriptorManager>().As<IShellDescriptorManager>().SingleInstance();
            builder.RegisterType<ModuleRecipeHandlerTest.StubDataMigrationManager>().As<IDataMigrationManager>();
            builder.RegisterInstance(_packagesInRepository).As<IPackagingSourceManager>();
            builder.RegisterInstance(_packageManager).As<IPackageManager>();
            builder.RegisterType<ShellStateManager>().As<IShellStateManager>().SingleInstance();
            builder.RegisterInstance(_testVirtualPathProvider).As<IVirtualPathProvider>();
            builder.RegisterType<StubEventBus>().As<IEventBus>().SingleInstance();
            builder.RegisterType<ThemeService>().As<IThemeService>();
            builder.RegisterType<StubOrchardServices>().As<IOrchardServices>();
            builder.RegisterType<StubSiteThemeService>().As<ISiteThemeService>();
            builder.RegisterType<ThemeRecipeHandler>();
            builder.RegisterSource(new EventsRegistrationSource());
        }

        [Test]
        public void ExecuteRecipeStepTest() {
            _folders.Manifests.Add("SuperWiki", @"
Name: SuperWiki
Version: 1.0.3
OrchardVersion: 1
Features:
    SuperWiki: 
        Description: My super wiki theme for Orchard.
");
            _packagesInRepository.AddPublishedPackage(new PublishedPackage {
                Id = "Orchard.Theme.SuperWiki",
                PackageType = DefaultExtensionTypes.Theme,
                Title = "SuperWiki",
                Version = "1.0.3",
                IsLatestVersion = true,
            });

            IShellDescriptorManager shellDescriptorManager = _container.Resolve<IShellDescriptorManager>();
            // No features enabled
            shellDescriptorManager.UpdateShellDescriptor(0,
                                                         Enumerable.Empty<ShellFeature>(),
                                                         Enumerable.Empty<ShellParameter>());

            ThemeRecipeHandler themeRecipeHandler = _container.Resolve<ThemeRecipeHandler>();

            RecipeContext recipeContext = new RecipeContext { RecipeStep = new RecipeStep { Name = "Theme", Step = new XElement("SuperWiki") } };
            recipeContext.RecipeStep.Step.Add(new XAttribute("packageId", "Orchard.Theme.SuperWiki"));
            recipeContext.RecipeStep.Step.Add(new XAttribute("repository", "test"));

            IFeatureManager featureManager = _container.Resolve<IFeatureManager>();
            IEnumerable<FeatureDescriptor> enabledFeatures = featureManager.GetEnabledFeatures();
            Assert.That(enabledFeatures.Count(), Is.EqualTo(0));
            themeRecipeHandler.ExecuteRecipeStep(recipeContext);

            // without setting enable no feature should be activated...
            featureManager.GetEnabledFeatures();
            Assert.That(enabledFeatures.Count(), Is.EqualTo(0));

            // Adding enable the feature should get active
            recipeContext.RecipeStep.Step.Add(new XAttribute("enable", true));
            themeRecipeHandler.ExecuteRecipeStep(recipeContext);

            enabledFeatures = featureManager.GetEnabledFeatures();
            Assert.That(enabledFeatures.FirstOrDefault(feature => feature.Id.Equals("SuperWiki")), Is.Not.Null);
            Assert.That(enabledFeatures.Count(), Is.EqualTo(1));
            Assert.That(recipeContext.Executed, Is.True);
        }

        [Test]
        public void ExecuteRecipeStepNeedsNameTest() {
            _folders.Manifests.Add("SuperWiki", @"
Name: SuperWiki
Version: 1.0.3
OrchardVersion: 1
Features:
    SuperWiki: 
        Description: My super wiki module for Orchard.
");

            ThemeRecipeHandler themeRecipeHandler = _container.Resolve<ThemeRecipeHandler>();

            RecipeContext recipeContext = new RecipeContext { RecipeStep = new RecipeStep { Name = "Theme", Step = new XElement("SuperWiki") } };
            recipeContext.RecipeStep.Step.Add(new XAttribute("repository", "test"));

            Assert.Throws(typeof (InvalidOperationException), () => themeRecipeHandler.ExecuteRecipeStep(recipeContext));
        }

        [Test]
        public void ExecuteRecipeStepWithRepositoryAndVersionNotLatestTest() {
            _packagesInRepository.AddPublishedPackage(new PublishedPackage {
                Id = "Orchard.Theme.SuperWiki",
                PackageType = DefaultExtensionTypes.Theme,
                Title = "SuperWiki",
                Version = "1.0.3",
                IsLatestVersion = true,
            });
            _packagesInRepository.AddPublishedPackage(new PublishedPackage {
                Id = "Orchard.Theme.SuperWiki",
                PackageType = DefaultExtensionTypes.Theme,
                Title = "SuperWiki",
                Version = "1.0.2",
                IsLatestVersion = false,
            });

            ThemeRecipeHandler themeRecipeHandler = _container.Resolve<ThemeRecipeHandler>();

            RecipeContext recipeContext = new RecipeContext { RecipeStep = new RecipeStep { Name = "Theme", Step = new XElement("SuperWiki") } };
            recipeContext.RecipeStep.Step.Add(new XAttribute("packageId", "Orchard.Theme.SuperWiki"));
            recipeContext.RecipeStep.Step.Add(new XAttribute("repository", "test"));
            recipeContext.RecipeStep.Step.Add(new XAttribute("version", "1.0.2"));

            themeRecipeHandler.ExecuteRecipeStep(recipeContext);

            var installedPackage = _packageManager.GetInstalledPackages().FirstOrDefault(info => info.ExtensionName == "Orchard.Theme.SuperWiki");
            Assert.That(installedPackage, Is.Not.Null);
            Assert.That(installedPackage.ExtensionVersion, Is.EqualTo("1.0.2"));
            Assert.That(recipeContext.Executed, Is.True);
        }

        internal class StubSiteThemeService : ISiteThemeService {
            public ExtensionDescriptor GetSiteTheme() {
                throw new NotImplementedException();
            }

            public void SetSiteTheme(string themeName) {
                throw new NotImplementedException();
            }

            public string GetCurrentThemeName() {
                throw new NotImplementedException();
            }
        }
    }
}

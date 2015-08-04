using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Autofac;
using NuGet;
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
using Orchard.Packaging.GalleryServer;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Providers.Executors;
using Orchard.Recipes.Services;
using Orchard.Tests.Environment.Extensions;
using Orchard.Tests.Environment.Features;
using Orchard.Tests.Stubs;
using IPackageManager = Orchard.Packaging.Services.IPackageManager;

namespace Orchard.Tests.Modules.Recipes.RecipeHandlers {
    [TestFixture]
    public class ModuleStepTest : DatabaseEnabledTestsBase {
        private ExtensionManagerTests.StubFolders _folders;
        private StubPackagingSourceManager _packagesInRepository;
        private StubPackageManager _packageManager;

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
            builder.RegisterInstance(new ShellSettings { Name = "Default" });

            _folders = new ExtensionManagerTests.StubFolders();
            _packagesInRepository = new StubPackagingSourceManager();
            _packageManager = new StubPackageManager();
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<RecipeExecutionLogger>().AsSelf();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<FeatureManager>().As<IFeatureManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();
            builder.RegisterType<ShellDescriptorManager>().As<IShellDescriptorManager>().SingleInstance();
            builder.RegisterType<StubDataMigrationManager>().As<IDataMigrationManager>();
            builder.RegisterInstance(_packagesInRepository).As<IPackagingSourceManager>();
            builder.RegisterInstance(_packageManager).As<IPackageManager>();
            builder.RegisterType<ShellStateManager>().As<IShellStateManager>().SingleInstance();
            builder.RegisterType<StubEventBus>().As<IEventBus>().SingleInstance();
            builder.RegisterType<ModuleStep>();
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
        Description: My super wiki module for Orchard.
");
            _packagesInRepository.AddPublishedPackage(new PublishedPackage {
                Id = "Orchard.Module.SuperWiki",
                PackageType = DefaultExtensionTypes.Module,
                Title = "SuperWiki",
                Version = "1.0.3",
                IsLatestVersion = true,
            });

            var shellDescriptorManager = _container.Resolve<IShellDescriptorManager>();
            // No features enabled
            shellDescriptorManager.UpdateShellDescriptor(
                0,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            var moduleStep = _container.Resolve<ModuleStep>();
            var recipeExecutionContext = new RecipeExecutionContext {RecipeStep = new RecipeStep(id: "1", recipeName: "Test", name: "Module", step: new XElement("SuperWiki")) };
            recipeExecutionContext.RecipeStep.Step.Add(new XAttribute("packageId", "Orchard.Module.SuperWiki"));
            recipeExecutionContext.RecipeStep.Step.Add(new XAttribute("repository", "test"));

            var featureManager = _container.Resolve<IFeatureManager>();
            var enabledFeatures = featureManager.GetEnabledFeatures();
            Assert.That(enabledFeatures.Count(), Is.EqualTo(0));
            moduleStep.Execute(recipeExecutionContext);


            var availableFeatures = featureManager.GetAvailableFeatures().FirstOrDefault(x => x.Id == "SuperWiki");
            Assert.That(availableFeatures.Id, Is.EqualTo("SuperWiki"));
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

            var moduleStep = _container.Resolve<ModuleStep>();
            var recipeContext = new RecipeContext { RecipeStep = new RecipeStep(id: "1", recipeName: "Test", name: "Module", step: new XElement("SuperWiki")) };
            var recipeExecutionContext = new RecipeExecutionContext { RecipeStep = recipeContext.RecipeStep };
            recipeContext.RecipeStep.Step.Add(new XAttribute("repository", "test"));

            Assert.Throws(typeof (InvalidOperationException), () => moduleStep.Execute(recipeExecutionContext));
        }

        [Test]
        public void ExecuteRecipeStepWithRepositoryAndVersionNotLatestTest() {
            _packagesInRepository.AddPublishedPackage(new PublishedPackage {
                Id = "Orchard.Module.SuperWiki",
                PackageType = DefaultExtensionTypes.Module,
                Title = "SuperWiki",
                Version = "1.0.3",
                IsLatestVersion = true,
            });
            _packagesInRepository.AddPublishedPackage(new PublishedPackage {
                Id = "Orchard.Module.SuperWiki",
                PackageType = DefaultExtensionTypes.Module,
                Title = "SuperWiki",
                Version = "1.0.2",
                IsLatestVersion = false,
            });

            var moduleStep = _container.Resolve<ModuleStep>();
            var recipeExecutionContext = new RecipeExecutionContext { RecipeStep = new RecipeStep(id: "1", recipeName: "Test", name: "Module", step: new XElement("SuperWiki")) };

            recipeExecutionContext.RecipeStep.Step.Add(new XAttribute("packageId", "Orchard.Module.SuperWiki"));
            recipeExecutionContext.RecipeStep.Step.Add(new XAttribute("repository", "test"));
            recipeExecutionContext.RecipeStep.Step.Add(new XAttribute("version", "1.0.2"));

            moduleStep.Execute(recipeExecutionContext);

            var installedPackage = _packageManager.GetInstalledPackages().FirstOrDefault(info => info.ExtensionName == "Orchard.Module.SuperWiki");
            Assert.That(installedPackage, Is.Not.Null);
            Assert.That(installedPackage.ExtensionVersion, Is.EqualTo("1.0.2"));
        }

        internal class StubPackagingSourceManager : IPackagingSourceManager {
            private readonly List<PublishedPackage> _publishedPackages = new List<PublishedPackage>();

            public IEnumerable<PackagingSource> GetSources() {
                return Enumerable.Empty<PackagingSource>();
            }

            public int AddSource(string feedTitle, string feedUrl) {
                throw new NotImplementedException();
            }

            public void RemoveSource(int id) {
                throw new NotImplementedException();
            }

            public IEnumerable<PackagingEntry> GetExtensionList(bool includeScreenshots, PackagingSource packagingSource = null, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query = null) {
                return query(_publishedPackages.AsQueryable()).Select(package => CreatePackagingEntry(package));
            }

            public int GetExtensionCount(PackagingSource packagingSource = null, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query = null) {
                throw new NotImplementedException();
            }

            public void AddPublishedPackage(PublishedPackage package) {
                _publishedPackages.Add(package);
            }

            private static PackagingEntry CreatePackagingEntry(PublishedPackage package) {
                return new PackagingEntry {
                    PackageId = package.Id,
                    Title = package.Title,
                    Version = package.Version,
                };
            }
        }

        internal class StubPackageManager : IPackageManager {
            private readonly IList<PackageInfo> _installedPackages = new List<PackageInfo>();

            public IEnumerable<PackageInfo> GetInstalledPackages() {
                return _installedPackages;
            }

            public PackageData Harvest(string extensionName) {
                throw new NotImplementedException();
            }

            public PackageInfo Install(IPackage package, string location, string applicationPath) {
                return null;
            }

            public PackageInfo Install(string packageId, string version, string location, string applicationPath) {
                var package = new PackageInfo {
                    ExtensionName = packageId,
                    ExtensionVersion = version,
                };
                _installedPackages.Add(package);
                return package;
            }

            public void Uninstall(string packageId, string applicationPath) {
            }

            public ExtensionDescriptor GetExtensionDescriptor(IPackage package, string extensionType) {
                throw new NotImplementedException();
            }
        }

        internal class StubDataMigrationManager : IDataMigrationManager {
            public bool IsFeatureAlreadyInstalled(string feature) {
                return true;
            }

            public IEnumerable<string> GetFeaturesThatNeedUpdate() {
                return Enumerable.Empty<string>();
            }

            public void Update(string feature) {
            }

            public void Update(IEnumerable<string> features) {
            }

            public void Uninstall(string feature) {
            }
        }
    }
}

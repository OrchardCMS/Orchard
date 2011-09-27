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
using Orchard.Recipes.RecipeHandlers;
using Orchard.Recipes.Services;
using Orchard.Tests.Environment.Extensions;
using Orchard.Tests.Environment.Features;
using Orchard.Tests.Stubs;
using IPackageManager = Orchard.Packaging.Services.IPackageManager;

namespace Orchard.Tests.Modules.Recipes.RecipeHandlers {
    [TestFixture]
    public class ModuleRecipeHandlerTest : DatabaseEnabledTestsBase {
        private ExtensionManagerTests.StubFolders _folders;

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
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<FeatureManager>().As<IFeatureManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();
            builder.RegisterType<ShellDescriptorManager>().As<IShellDescriptorManager>().SingleInstance();
            builder.RegisterType<StubDataMigrationManager>().As<IDataMigrationManager>();
            builder.RegisterType<StubPackagingSourceManager>().As<IPackagingSourceManager>();
            builder.RegisterType<StubPackageManager>().As<IPackageManager>();
            builder.RegisterType<ShellStateManager>().As<IShellStateManager>().SingleInstance();
            builder.RegisterType<StubEventBus>().As<IEventBus>().SingleInstance();
            builder.RegisterType<ModuleRecipeHandler>();
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

            IShellDescriptorManager shellDescriptorManager = _container.Resolve<IShellDescriptorManager>();
            // No features enabled
            shellDescriptorManager.UpdateShellDescriptor(0,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            ModuleRecipeHandler moduleRecipeHandler = _container.Resolve<ModuleRecipeHandler>();

            RecipeContext recipeContext = new RecipeContext { RecipeStep = new RecipeStep { Name = "Module", Step = new XElement("SuperWiki") } };
            recipeContext.RecipeStep.Step.Add(new XAttribute("packageId", "Orchard.Module.SuperWiki"));
            recipeContext.RecipeStep.Step.Add(new XAttribute("repository", "test"));

            IFeatureManager featureManager = _container.Resolve<IFeatureManager>();
            IEnumerable<FeatureDescriptor> enabledFeatures = featureManager.GetEnabledFeatures();
            Assert.That(enabledFeatures.Count(), Is.EqualTo(0));
            moduleRecipeHandler.ExecuteRecipeStep(recipeContext);


            var availableFeatures = featureManager.GetAvailableFeatures().Where(x => x.Id == "SuperWiki").FirstOrDefault();
            Assert.That(availableFeatures.Id, Is.EqualTo("SuperWiki"));
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

            ModuleRecipeHandler moduleRecipeHandler = _container.Resolve<ModuleRecipeHandler>();

            RecipeContext recipeContext = new RecipeContext { RecipeStep = new RecipeStep { Name = "Module", Step = new XElement("SuperWiki") } };
            recipeContext.RecipeStep.Step.Add(new XAttribute("repository", "test"));

            Assert.Throws(typeof(InvalidOperationException), () => moduleRecipeHandler.ExecuteRecipeStep(recipeContext));
        }

        internal class StubPackagingSourceManager : IPackagingSourceManager {
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
                return new[] {
                    new PackagingEntry {
                        PackageId = "Orchard.Module.SuperWiki",
                        Title = "SuperWiki",
                        Version = "1.0.3"
                    }
                };
            }

            public int GetExtensionCount(PackagingSource packagingSource = null, Func<IQueryable<PublishedPackage>, IQueryable<PublishedPackage>> query = null) {
                throw new NotImplementedException();
            }
        }

        internal class StubPackageManager : IPackageManager {
            public PackageData Harvest(string extensionName) {
                throw new NotImplementedException();
            }

            public PackageInfo Install(IPackage package, string location, string applicationPath) {
                return null;
            }

            public PackageInfo Install(string packageId, string version, string location, string applicationPath) {
                return null;
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

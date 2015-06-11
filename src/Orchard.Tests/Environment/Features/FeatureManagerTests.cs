using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Core.Settings.Descriptor;
using Orchard.Core.Settings.Descriptor.Records;
using Orchard.Core.Settings.State;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Features;
using Orchard.Environment.State;
using Orchard.Events;
using Orchard.Tests.Environment.Extensions;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Environment.Features {
    [TestFixture]
    public class FeatureManagerTests : DatabaseEnabledTestsBase {
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
            _folders = new ExtensionManagerTests.StubFolders();
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<FeatureManager>().As<IFeatureManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();
            builder.RegisterType<ShellDescriptorManager>().As<IShellDescriptorManager>().SingleInstance();
            builder.RegisterType<ShellStateManager>().As<IShellStateManager>().SingleInstance();
            builder.RegisterType<StubEventBus>().As<IEventBus>().SingleInstance();
            builder.RegisterSource(new EventsRegistrationSource());

            builder.RegisterInstance(new ShellSettings { Name = "Default" });
        }

        [Test]
        public void EnableFeaturesTest() {
            _folders.Manifests.Add("SuperWiki", @"
Name: SuperWiki
Version: 1.0.3
OrchardVersion: 1
Features:
    SuperWiki: 
        Description: My super wiki module for Orchard.
");

            // Initialize the shell descriptor with 0 features
            IShellDescriptorManager shellDescriptorManager = _container.Resolve<IShellDescriptorManager>();
            IFeatureManager featureManager = _container.Resolve<IFeatureManager>();

            shellDescriptorManager.UpdateShellDescriptor(0,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            IEnumerable<string> featuresToEnable = new [] { "SuperWiki" };

            // Enable all features
            IEnumerable<string> enabledFeatures = featureManager.EnableFeatures(featuresToEnable);

            Assert.That(enabledFeatures, Is.EqualTo(featuresToEnable));
            Assert.That(featureManager.GetEnabledFeatures().Count(), Is.EqualTo(1));
        }

        [Test]
        public void EnableFeaturesWithDependenciesTest() {
            _folders.Manifests.Add("SuperWiki", @"
Name: SuperWiki
Version: 1.0.3
OrchardVersion: 1
Features:
    SuperWiki: 
        Description: My super wiki module for Orchard.
        Dependencies: SuperWikiDep
    SuperWikiDep:
        Description: My super wiki module for Orchard dependency.
");

            // Initialize the shell descriptor with 0 features
            IShellDescriptorManager shellDescriptorManager = _container.Resolve<IShellDescriptorManager>();
            IFeatureManager featureManager = _container.Resolve<IFeatureManager>();

            shellDescriptorManager.UpdateShellDescriptor(0,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            IEnumerable<string> featuresToEnable = new[] { "SuperWiki" };

            // Try to enable without forcing dependencies should fail
            IEnumerable<string> enabledFeatures = featureManager.EnableFeatures(featuresToEnable, false);
            Assert.That(enabledFeatures.Count(), Is.EqualTo(0));
            Assert.That(featureManager.GetEnabledFeatures().Count(), Is.EqualTo(0));

            // Enabling while forcing dependencies should succeed.
            enabledFeatures = featureManager.EnableFeatures(featuresToEnable, true);
            Assert.That(enabledFeatures.Contains("SuperWiki"), Is.True);
            Assert.That(enabledFeatures.Contains("SuperWikiDep"), Is.True);
            Assert.That(featureManager.GetEnabledFeatures().Count(), Is.EqualTo(2));
        }

        [Test]
        public void DisableFeaturesTest() {
            _folders.Manifests.Add("SuperWiki", @"
Name: SuperWiki
Version: 1.0.3
OrchardVersion: 1
Features:
    SuperWiki: 
        Description: My super wiki module for Orchard.
");

            // Initialize the shell descriptor with 0 features
            IShellDescriptorManager shellDescriptorManager = _container.Resolve<IShellDescriptorManager>();
            IFeatureManager featureManager = _container.Resolve<IFeatureManager>();

            shellDescriptorManager.UpdateShellDescriptor(0,
                new [] { new ShellFeature { Name = "SuperWiki" } },
                Enumerable.Empty<ShellParameter>());

            IEnumerable<string> featuresToDisable = new [] { "SuperWiki" };

            // Disable the feature
            featureManager.DisableFeatures(featuresToDisable);
            Assert.That(featureManager.GetEnabledFeatures().Count(), Is.EqualTo(0));
        }

        [Test]
        public void DisableFeaturesWithDependenciesTest() {
            _folders.Manifests.Add("SuperWiki", @"
Name: SuperWiki
Version: 1.0.3
OrchardVersion: 1
Features:
    SuperWiki: 
        Description: My super wiki module for Orchard.
        Dependencies: SuperWikiDep
    SuperWikiDep:
        Description: My super wiki module for Orchard dependency.
");

            // Initialize the shell descriptor with 0 features
            IShellDescriptorManager shellDescriptorManager = _container.Resolve<IShellDescriptorManager>();
            IFeatureManager featureManager = _container.Resolve<IFeatureManager>();

            shellDescriptorManager.UpdateShellDescriptor(0,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            // Enable both features by relying on the dependency
            Assert.That(featureManager.EnableFeatures(new [] { "SuperWiki"}, true).Count(), Is.EqualTo(2));

            IEnumerable<string> featuresToDisable = new[] { "SuperWikiDep" };

            // Try to enable without forcing dependencies should fail
            IEnumerable<string> disabledFeatures = featureManager.DisableFeatures(featuresToDisable, false);
            Assert.That(disabledFeatures.Count(), Is.EqualTo(0));
            Assert.That(featureManager.GetEnabledFeatures().Count(), Is.EqualTo(2));

            // Enabling while forcing dependencies should succeed.
            disabledFeatures = featureManager.DisableFeatures(featuresToDisable, true);
            Assert.That(disabledFeatures.Contains("SuperWiki"), Is.True);
            Assert.That(disabledFeatures.Contains("SuperWikiDep"), Is.True);
            Assert.That(featureManager.GetEnabledFeatures().Count(), Is.EqualTo(0));
        }
    }

    public class StubEventBus : IEventBus {
        public string LastMessageName { get; set; }
        public IDictionary<string, object> LastEventData { get; set; }

        public IEnumerable Notify(string messageName, IDictionary<string, object> eventData) {
            LastMessageName = messageName;
            LastEventData = eventData;
            return new object[0];
        }
    }
}

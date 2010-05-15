using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Models;
using Orchard.Tests.Extensions.ExtensionTypes;
using Yaml.Grammar;

namespace Orchard.Tests.Environment.Extensions {
    [TestFixture]
    public class ExtensionManagerTests {
        private IContainer _container;
        private IExtensionManager _manager;
        private StubFolders _folders;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            _folders = new StubFolders();
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            _container = builder.Build();
            _manager = _container.Resolve<IExtensionManager>();
        }

        public class StubFolders : IExtensionFolders {
            public StubFolders() {
                Manifests = new Dictionary<string, string>();
            }

            public IDictionary<string, string> Manifests { get; set; }

            public IEnumerable<string> ListNames() {
                return Manifests.Keys;
            }

            public ParseResult ParseManifest(string name) {
                var parser = new YamlParser();
                bool success;
                var stream = parser.ParseYamlStream(new TextInput(Manifests[name]), out success);
                if (success) {
                    return new ParseResult {
                        Location = "~/InMemory",
                        Name = name,
                        YamlDocument = stream.Documents.Single()
                    };
                }
                return null;
            }

            public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
                throw new NotImplementedException();
            }
        }

        public class StubLoaders : IExtensionLoader {
            #region Implementation of IExtensionLoader

            public int Order {
                get { return 1; }
            }

            public ExtensionEntry Load(ExtensionDescriptor descriptor) {
                return new ExtensionEntry { Descriptor = descriptor, ExportedTypes = new[] { typeof(Alpha), typeof(Beta), typeof(Phi) } };
            }

            #endregion
        }


        [Test]
        public void AvailableExtensionsShouldFollowCatalogLocations() {
            _folders.Manifests.Add("foo", "name: Foo");
            _folders.Manifests.Add("bar", "name: Bar");
            _folders.Manifests.Add("frap", "name: Frap");
            _folders.Manifests.Add("quad", "name: Quad");

            var available = _manager.AvailableExtensions();

            Assert.That(available.Count(), Is.EqualTo(4));
            Assert.That(available, Has.Some.Property("Name").EqualTo("foo"));
        }

        [Test]
        public void ExtensionDescriptorsShouldHaveNameAndVersion() {

            _folders.Manifests.Add("Sample", @"
name: Sample Extension
version: 2.x
");

            var descriptor = _manager.AvailableExtensions().Single();
            Assert.That(descriptor.Name, Is.EqualTo("Sample"));
            Assert.That(descriptor.DisplayName, Is.EqualTo("Sample Extension"));
            Assert.That(descriptor.Version, Is.EqualTo("2.x"));
        }

        [Test]
        public void ExtensionDescriptorsShouldBeParsedForMinimalModuleTxt() {

            _folders.Manifests.Add("SuperWiki", @"
name: SuperWiki
version: 1.0.3
orchardversion: 1
features:
  SuperWiki: 
    Description: My super wiki module for Orchard.
");

            var descriptor = _manager.AvailableExtensions().Single();
            Assert.That(descriptor.Name, Is.EqualTo("SuperWiki"));
            Assert.That(descriptor.Version, Is.EqualTo("1.0.3"));
            Assert.That(descriptor.OrchardVersion, Is.EqualTo("1"));
            Assert.That(descriptor.Features.Count(), Is.EqualTo(1));
            Assert.That(descriptor.Features.First().Name, Is.EqualTo("SuperWiki"));
            Assert.That(descriptor.Features.First().Extension.Name, Is.EqualTo("SuperWiki"));
            Assert.That(descriptor.Features.First().Description, Is.EqualTo("My super wiki module for Orchard."));
        }

        [Test]
        public void ExtensionDescriptorsShouldBeParsedForCompleteModuleTxt() {

            _folders.Manifests.Add("MyCompany.AnotherWiki", @"
name: AnotherWiki
author: Coder Notaprogrammer
website: http://anotherwiki.codeplex.com
version: 1.2.3
orchardversion: 1
features:
  AnotherWiki: 
    Description: My super wiki module for Orchard.
    Dependencies: Versioning, Search
    Category: Content types
  AnotherWiki Editor:
    Description: A rich editor for wiki contents.
    Dependencies: TinyMCE, AnotherWiki
    Category: Input methods
  AnotherWiki DistributionList:
    Description: Sends e-mail alerts when wiki contents gets published.
    Dependencies: AnotherWiki, Email Subscriptions
    Category: Email
  AnotherWiki Captcha:
    Description: Kills spam. Or makes it zombie-like.
    Dependencies: AnotherWiki, reCaptcha
    Category: Spam
");

            var descriptor = _manager.AvailableExtensions().Single();
            Assert.That(descriptor.Name, Is.EqualTo("MyCompany.AnotherWiki"));
            Assert.That(descriptor.DisplayName, Is.EqualTo("AnotherWiki"));
            Assert.That(descriptor.Author, Is.EqualTo("Coder Notaprogrammer"));
            Assert.That(descriptor.WebSite, Is.EqualTo("http://anotherwiki.codeplex.com"));
            Assert.That(descriptor.Version, Is.EqualTo("1.2.3"));
            Assert.That(descriptor.OrchardVersion, Is.EqualTo("1"));
            Assert.That(descriptor.Features.Count(), Is.EqualTo(5));
            foreach (var featureDescriptor in descriptor.Features) {
                switch (featureDescriptor.Name) {
                    case "AnotherWiki":
                        Assert.That(featureDescriptor.Extension, Is.SameAs(descriptor));
                        Assert.That(featureDescriptor.Description, Is.EqualTo("My super wiki module for Orchard."));
                        Assert.That(featureDescriptor.Category, Is.EqualTo("Content types"));
                        Assert.That(featureDescriptor.Dependencies.Count(), Is.EqualTo(2));
                        Assert.That(featureDescriptor.Dependencies.Contains("Versioning"));
                        Assert.That(featureDescriptor.Dependencies.Contains("Search"));
                        break;
                    case "AnotherWiki Editor":
                        Assert.That(featureDescriptor.Extension, Is.SameAs(descriptor));
                        Assert.That(featureDescriptor.Description, Is.EqualTo("A rich editor for wiki contents."));
                        Assert.That(featureDescriptor.Category, Is.EqualTo("Input methods"));
                        Assert.That(featureDescriptor.Dependencies.Count(), Is.EqualTo(2));
                        Assert.That(featureDescriptor.Dependencies.Contains("TinyMCE"));
                        Assert.That(featureDescriptor.Dependencies.Contains("AnotherWiki"));
                        break;
                    case "AnotherWiki DistributionList":
                        Assert.That(featureDescriptor.Extension, Is.SameAs(descriptor));
                        Assert.That(featureDescriptor.Description, Is.EqualTo("Sends e-mail alerts when wiki contents gets published."));
                        Assert.That(featureDescriptor.Category, Is.EqualTo("Email"));
                        Assert.That(featureDescriptor.Dependencies.Count(), Is.EqualTo(2));
                        Assert.That(featureDescriptor.Dependencies.Contains("AnotherWiki"));
                        Assert.That(featureDescriptor.Dependencies.Contains("Email Subscriptions"));
                        break;
                    case "AnotherWiki Captcha":
                        Assert.That(featureDescriptor.Extension, Is.SameAs(descriptor));
                        Assert.That(featureDescriptor.Description, Is.EqualTo("Kills spam. Or makes it zombie-like."));
                        Assert.That(featureDescriptor.Category, Is.EqualTo("Spam"));
                        Assert.That(featureDescriptor.Dependencies.Count(), Is.EqualTo(2));
                        Assert.That(featureDescriptor.Dependencies.Contains("AnotherWiki"));
                        Assert.That(featureDescriptor.Dependencies.Contains("reCaptcha"));
                        break;
                    // default feature.
                    case "MyCompany.AnotherWiki":
                        Assert.That(featureDescriptor.Extension, Is.SameAs(descriptor));
                        break;
                    default:
                        Assert.Fail("Features not parsed correctly");
                        break;
                }
            }
        }

        [Test]
        public void ExtensionManagerShouldLoadFeatures() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders();

            extensionFolder.Manifests.Add("TestModule", @"
name: TestModule
version: 1.0.3
orchardversion: 1
features:
  TestModule: 
    Description: My test module for Orchard.
  TestFeature:
    Description: Contains the Phi type.
");

            ExtensionManager extensionManager = new ExtensionManager(new[] { extensionFolder }, new[] { extensionLoader });
            var testFeature = extensionManager.AvailableExtensions()
                .SelectMany(x => x.Features);

            var features = extensionManager.LoadFeatures(testFeature);
            var types = features.SelectMany(x => x.ExportedTypes);

            Assert.That(types.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        public void ExtensionManagerFeaturesContainNonAbstractClasses() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders();

            extensionFolder.Manifests.Add("TestModule", @"
name: TestModule
version: 1.0.3
orchardversion: 1
features:
  TestModule: 
    Description: My test module for Orchard.
  TestFeature:
    Description: Contains the Phi type.
");

            ExtensionManager extensionManager = new ExtensionManager(new[] { extensionFolder }, new[] { extensionLoader });
            var testFeature = extensionManager.AvailableExtensions()
                .SelectMany(x => x.Features);

            var features = extensionManager.LoadFeatures(testFeature);
            var types = features.SelectMany(x => x.ExportedTypes);

            foreach (var type in types) {
                Assert.That(type.IsClass);
                Assert.That(!type.IsAbstract);
            }
        }

        [Test]
        public void ExtensionManagerShouldThrowIfFeatureDoesNotExist() {
            var featureDescriptor = new FeatureDescriptor { Name = "NoSuchFeature" };
            Assert.Throws<ArgumentException>(() => _manager.LoadFeatures(new [] { featureDescriptor }));
        }

        [Test]
        public void ExtensionManagerTestFeatureAttribute() {
            var extensionManager = new Moq.Mock<IExtensionManager>();
            var extensionEntry =  new ExtensionEntry {
                                                            Descriptor = new ExtensionDescriptor { Name = "Module"},
                                                            ExportedTypes = new[] { typeof(Alpha), typeof(Beta), typeof(Phi) }
                                                        };
            extensionEntry.Descriptor.Features = new[] {
                                                           new FeatureDescriptor
                                                           {Name = "Module", Extension = extensionEntry.Descriptor},
                                                           new FeatureDescriptor
                                                           {Name = "TestFeature", Extension = extensionEntry.Descriptor}
                                                       };
            extensionManager.Setup(x => x.ActiveExtensions_Obsolete()).Returns(new[] {extensionEntry});

            foreach (var type in extensionManager.Object.ActiveExtensions_Obsolete().SelectMany(x => x.ExportedTypes)) {
                foreach (OrchardFeatureAttribute featureAttribute in type.GetCustomAttributes(typeof(OrchardFeatureAttribute), false)) {
                    Assert.That(featureAttribute.FeatureName, Is.EqualTo("TestFeature"));
                }
            }
        }

        [Test]
        public void ExtensionManagerLoadFeatureReturnsTypesFromSpecificFeaturesWithFeatureAttribute() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders();

            extensionFolder.Manifests.Add("TestModule", @"
name: TestModule
version: 1.0.3
orchardversion: 1
features:
  TestModule: 
    Description: My test module for Orchard.
  TestFeature:
    Description: Contains the Phi type.
");

            ExtensionManager extensionManager = new ExtensionManager(new[] { extensionFolder }, new[] { extensionLoader });
            var testFeature = extensionManager.AvailableExtensions()
                .SelectMany(x => x.Features)
                .Single(x => x.Name == "TestFeature");

            foreach (var feature in extensionManager.LoadFeatures(new[] { testFeature })) {
                foreach (var type in feature.ExportedTypes) {
                    Assert.That(type == typeof(Phi));
                }
            }
        }

        [Test]
        public void ExtensionManagerLoadFeatureDoesNotReturnTypesFromNonMatchingFeatures() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders();

            extensionFolder.Manifests.Add("TestModule", @"
name: TestModule
version: 1.0.3
orchardversion: 1
features:
  TestModule: 
    Description: My test module for Orchard.
  TestFeature:
    Description: Contains the Phi type.
");

            ExtensionManager extensionManager = new ExtensionManager(new[] { extensionFolder }, new[] { extensionLoader });
            var testModule = extensionManager.AvailableExtensions()
                .SelectMany(x => x.Features)
                .Single(x => x.Name == "TestModule");

            foreach (var feature in extensionManager.LoadFeatures(new [] { testModule })) {
                foreach (var type in feature.ExportedTypes) {
                    Assert.That(type != typeof(Phi));
                    Assert.That((type == typeof(Alpha) || (type == typeof(Beta))));
                }
            }
        }

        [Test]
        public void ModuleNameIsIntroducedAsFeatureImplicitly() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders();

            extensionFolder.Manifests.Add("Minimalistic", @"
name: Minimalistic
version: 1.0.3
orchardversion: 1
");

            ExtensionManager extensionManager = new ExtensionManager(new[] { extensionFolder }, new[] { extensionLoader });
            var minimalisticModule = extensionManager.AvailableExtensions().Single(x => x.Name == "Minimalistic");

            Assert.That(minimalisticModule.Features.Count(), Is.EqualTo(1));
            Assert.That(minimalisticModule.Features.Single().Name, Is.EqualTo("Minimalistic"));
        }
    }
}
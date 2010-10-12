using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using NUnit.Framework;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Tests.DisplayManagement.Descriptors {
    [TestFixture]
    public class DefaultShapeTableManagerTests : ContainerTestBase {
        protected override void Register(Autofac.ContainerBuilder builder) {
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();

            var features = new [] {
                new FeatureDescriptor {
                    Name = "Theme1",
                    Extension = new ExtensionDescriptor {
                        Name = "Theme1",
                        ExtensionType = "Theme"
                    }
                },
                new FeatureDescriptor {
                    Name = "DerivedTheme",
                    Extension = new ExtensionDescriptor {
                        Name = "DerivedTheme",
                        ExtensionType = "Theme",
                        BaseTheme = "BaseTheme"
                    }
                },
                new FeatureDescriptor {
                    Name = "BaseTheme",
                    Extension = new ExtensionDescriptor {
                        Name = "BaseTheme",
                        ExtensionType = "Theme"
                    }
                }
            };
            builder.RegisterInstance<IExtensionManager>(new TestExtensionManager(features));
            
            TestShapeProvider.FeatureShapes = new Dictionary<Feature, IEnumerable<string>> {
                { TestFeature(), new [] {"Hello"} },
                { Feature(features[0]), new [] {"Theme1Shape"} },
                { Feature(features[1]), new [] {"DerivedShape", "OverriddenShape"} },
                { Feature(features[2]), new [] {"BaseShape", "OverriddenShape"} }
            };

            builder.RegisterType<TestShapeProvider>().As<IShapeTableProvider>()
                .WithMetadata("Features", TestFeature())
                .As<TestShapeProvider>()
                .InstancePerLifetimeScope();
        }

        static Feature Feature(FeatureDescriptor descriptor) {
            return new Feature {
                Descriptor = descriptor
            };
        }

        static Feature TestFeature() {
            return new Feature {
                Descriptor = new FeatureDescriptor {
                    Name = "Testing",
                    Dependencies = Enumerable.Empty<string>(),
                    Extension = new ExtensionDescriptor {
                        Name = "Testing",
                        ExtensionType = "Module",
                    }
                }
            };
        }

        public class TestExtensionManager : IExtensionManager {
            private readonly IEnumerable<FeatureDescriptor> _availableFeautures;

            public TestExtensionManager(IEnumerable<FeatureDescriptor> availableFeautures) {
                _availableFeautures = availableFeautures;
            }

            public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
                throw new NotSupportedException();
            }

            public IEnumerable<FeatureDescriptor> AvailableFeatures() {
                return _availableFeautures;
            }

            public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
                throw new NotSupportedException();
            }

            public void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle) {
                throw new NotSupportedException();
            }

            public void UninstallExtension(string extensionType, string extensionName) {
                throw new NotSupportedException();
            }
        }

        public class TestShapeProvider : IShapeTableProvider {
            public static IDictionary<Feature, IEnumerable<string>> FeatureShapes;

            public Action<ShapeTableBuilder> Discover = x => { };

            void IShapeTableProvider.Discover(ShapeTableBuilder builder) {
                foreach (var pair in FeatureShapes) {
                    foreach (var shape in pair.Value) {
                        builder.Describe(shape).From(pair.Key).BoundAs(pair.Key.Descriptor.Name, null);
                    }
                }
                Discover(builder);
            }
        }

        [Test]
        public void ManagerCanBeResolved() {
            var manager = _container.Resolve<IShapeTableManager>();
            Assert.That(manager, Is.Not.Null);
        }

        [Test]
        public void DefaultShapeTableIsReturnedForNullOrEmpty() {
            var manager = _container.Resolve<IShapeTableManager>();
            var shapeTable1 = manager.GetShapeTable(null);
            var shapeTable2 = manager.GetShapeTable(string.Empty);
            Assert.That(shapeTable1.Descriptors["Hello"], Is.Not.Null);
            Assert.That(shapeTable2.Descriptors["Hello"], Is.Not.Null);
        }

        [Test]
        public void CallbackAlterationsContributeToDescriptor() {
            Action<ShapeCreatingContext> cb1 = x => { };
            Action<ShapeCreatedContext> cb2 = x => { };
            Action<ShapeDisplayingContext> cb3 = x => { };
            Action<ShapeDisplayedContext> cb4 = x => { };

            _container.Resolve<TestShapeProvider>().Discover =
                builder => builder.Describe("Foo").From(TestFeature())
                               .OnCreating(cb1)
                               .OnCreated(cb2)
                               .OnDisplaying(cb3)
                               .OnDisplayed(cb4);

            var manager = _container.Resolve<IShapeTableManager>();

            var foo = manager.GetShapeTable(null).Descriptors["Foo"];

            Assert.That(foo.Creating.Single(), Is.SameAs(cb1));
            Assert.That(foo.Created.Single(), Is.SameAs(cb2));
            Assert.That(foo.Displaying.Single(), Is.SameAs(cb3));
            Assert.That(foo.Displayed.Single(), Is.SameAs(cb4));
        }

        [Test]
        public void OnlyShapesFromTheGivenThemeAreProvided() {
            _container.Resolve<TestShapeProvider>();
            var manager = _container.Resolve<IShapeTableManager>();
            var table = manager.GetShapeTable("Theme1");
            Assert.IsTrue(table.Descriptors.ContainsKey("Theme1Shape"));
            Assert.IsFalse(table.Descriptors.ContainsKey("DerivedShape"));
            Assert.IsFalse(table.Descriptors.ContainsKey("BaseShape"));
        }

        [Test]
        public void ShapesFromTheBaseThemeAreProvided() {
            _container.Resolve<TestShapeProvider>();
            var manager = _container.Resolve<IShapeTableManager>();
            var table = manager.GetShapeTable("DerivedTheme");
            Assert.IsFalse(table.Descriptors.ContainsKey("Theme1Shape"));
            Assert.IsTrue(table.Descriptors.ContainsKey("DerivedShape"));
            Assert.IsTrue(table.Descriptors.ContainsKey("BaseShape"));
        }

        [Test]
        public void DerivedThemesCanOverrideBaseThemeShapeBindings() {
            _container.Resolve<TestShapeProvider>();
            var manager = _container.Resolve<IShapeTableManager>();
            var table = manager.GetShapeTable("DerivedTheme");
            Assert.IsTrue(table.Bindings.ContainsKey("OverriddenShape"));
            Assert.AreEqual("DerivedTheme", table.Descriptors["OverriddenShape"].BindingSource);
        }
    }
}
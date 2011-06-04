using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.DisplayManagement.Descriptors {
    [TestFixture]
    public class DefaultShapeTableManagerTests : ContainerTestBase {
        protected override void Register(ContainerBuilder builder) {
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();

            var features = new [] {
                new FeatureDescriptor {
                    Id = "Theme1",
                    Extension = new ExtensionDescriptor {
                        Id = "Theme1",
                        ExtensionType = DefaultExtensionTypes.Theme
                    }
                },
                new FeatureDescriptor {
                    Id = "DerivedTheme",
                    Extension = new ExtensionDescriptor {
                        Id = "DerivedTheme",
                        ExtensionType = DefaultExtensionTypes.Theme,
                        BaseTheme = "BaseTheme"
                    }
                },
                new FeatureDescriptor {
                    Id = "BaseTheme",
                    Extension = new ExtensionDescriptor {
                        Id = "BaseTheme",
                        ExtensionType = DefaultExtensionTypes.Theme
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
                .As<TestShapeProvider>()
                .InstancePerLifetimeScope();

            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();
        }

        static Feature Feature(FeatureDescriptor descriptor) {
            return new Feature {
                Descriptor = descriptor
            };
        }

        static Feature TestFeature() {
            return new Feature {
                Descriptor = new FeatureDescriptor {
                    Id = "Testing",
                    Dependencies = Enumerable.Empty<string>(),
                    Extension = new ExtensionDescriptor {
                        Id = "Testing",
                        ExtensionType = DefaultExtensionTypes.Module,
                    }
                }
            };
        }

        public class TestExtensionManager : IExtensionManager {
            private readonly IEnumerable<FeatureDescriptor> _availableFeautures;

            public TestExtensionManager(IEnumerable<FeatureDescriptor> availableFeautures) {
                _availableFeautures = availableFeautures;
            }

            public ExtensionDescriptor GetExtension(string name) {
                throw new NotImplementedException();
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
        }

        public class TestShapeProvider : IShapeTableProvider {
            public static IDictionary<Feature, IEnumerable<string>> FeatureShapes;

            public Action<ShapeTableBuilder> Discover = x => { };

            void IShapeTableProvider.Discover(ShapeTableBuilder builder) {
                foreach (var pair in FeatureShapes) {
                    foreach (var shape in pair.Value) {
                        builder.Describe(shape).From(pair.Key).BoundAs(pair.Key.Descriptor.Id, null);
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
        public void DefaultPlacementIsReturnedByDefault() {
            var manager = _container.Resolve<IShapeTableManager>();

            var hello = manager.GetShapeTable(null).Descriptors["Hello"];
            hello.DefaultPlacement = "Header:5";
            var result = hello.Placement(null);
            Assert.That(result.Location, Is.EqualTo("Header:5"));
        }

        [Test]
        public void DescribedPlacementIsReturnedIfNotNull() {

            _container.Resolve<TestShapeProvider>().Discover =
                builder => builder.Describe("Hello").From(TestFeature())
                    .Placement(ctx => ctx.DisplayType == "Detail" ? new PlacementInfo { Location = "Main" } : null)
                    .Placement(ctx => ctx.DisplayType == "Summary" ? new PlacementInfo { Location = "" } : null);

            var manager = _container.Resolve<IShapeTableManager>();
            var hello = manager.GetShapeTable(null).Descriptors["Hello"];
            var result1 = hello.Placement(new ShapePlacementContext { DisplayType = "Detail" });
            var result2 = hello.Placement(new ShapePlacementContext { DisplayType = "Summary" });
            var result3 = hello.Placement(new ShapePlacementContext { DisplayType = "Tile" });
            hello.DefaultPlacement = "Header:5";
            var result4 = hello.Placement(new ShapePlacementContext { DisplayType = "Detail" });
            var result5 = hello.Placement(new ShapePlacementContext { DisplayType = "Summary" });
            var result6 = hello.Placement(new ShapePlacementContext { DisplayType = "Tile" });
            
            Assert.That(result1.Location, Is.EqualTo("Main"));
            Assert.That(result2.Location, Is.EqualTo(""));
            Assert.That(result3.Location, Is.Null);
            Assert.That(result4.Location, Is.EqualTo("Main"));
            Assert.That(result5.Location, Is.EqualTo(""));
            Assert.That(result6.Location, Is.EqualTo("Header:5"));
        }
        
        [Test]
        public void TwoArgumentVariationDoesSameThing() {

            _container.Resolve<TestShapeProvider>().Discover =
                builder => builder.Describe("Hello").From(TestFeature())
                    .Placement(ctx => ctx.DisplayType == "Detail", new PlacementInfo { Location = "Main" })
                    .Placement(ctx => ctx.DisplayType == "Summary", new PlacementInfo { Location = "" });

            var manager = _container.Resolve<IShapeTableManager>();
            var hello = manager.GetShapeTable(null).Descriptors["Hello"];
            var result1 = hello.Placement(new ShapePlacementContext { DisplayType = "Detail" });
            var result2 = hello.Placement(new ShapePlacementContext { DisplayType = "Summary" });
            var result3 = hello.Placement(new ShapePlacementContext { DisplayType = "Tile" });
            hello.DefaultPlacement = "Header:5";
            var result4 = hello.Placement(new ShapePlacementContext { DisplayType = "Detail" });
            var result5 = hello.Placement(new ShapePlacementContext { DisplayType = "Summary" });
            var result6 = hello.Placement(new ShapePlacementContext { DisplayType = "Tile" });

            Assert.That(result1.Location, Is.EqualTo("Main"));
            Assert.That(result2.Location, Is.EqualTo(""));
            Assert.That(result3.Location, Is.Null);
            Assert.That(result4.Location, Is.EqualTo("Main"));
            Assert.That(result5.Location, Is.EqualTo(""));
            Assert.That(result6.Location, Is.EqualTo("Header:5"));
        }

        [Test]
        public void PathConstraintShouldMatch() {

            // all path have a trailing / as per the current implementation
            // todo: (sebros) find a way to 'use' the current implementation in DefaultContentDisplay.BindPlacement instead of emulating it

            var rules = new[] {
                Tuple.Create("~/my-blog", "~/my-blog/", true),
                Tuple.Create("~/my-blog/", "~/my-blog/", true),

                // star match
                Tuple.Create("~/my-blog*", "~/my-blog/", true),
                Tuple.Create("~/my-blog*", "~/my-blog/my-post/", true),
                Tuple.Create("~/my-blog/*", "~/my-blog/", true),
                Tuple.Create("~/my-blog/*", "~/my-blog123/", false),
                Tuple.Create("~/my-blog*", "~/my-blog123/", true)
            };

            foreach (var rule in rules) {
                var path = rule.Item1;
                var context = rule.Item2;
                var match = rule.Item3;

                _container.Resolve<TestShapeProvider>().Discover =
                    builder => builder.Describe("Hello").From(TestFeature())
                                   .Placement(ShapePlacementParsingStrategy.BuildPredicate(c => true, new KeyValuePair<string, string>("Path", path)), new PlacementInfo { Location = "Match" });

                var manager = _container.Resolve<IShapeTableManager>();
                var hello = manager.GetShapeTable(null).Descriptors["Hello"];
                var result = hello.Placement(new ShapePlacementContext {Path = context});

                if (match) {
                    Assert.That(result.Location, Is.EqualTo("Match"), String.Format("{0}|{1}", path, context));
                }
                else {
                    Assert.That(result.Location, Is.Null, String.Format("{0}|{1}", path, context));
                }
            }
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
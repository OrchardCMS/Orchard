using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Tests.DisplayManagement.Descriptors {
    [TestFixture]
    public class ShapeTemplateBindingStrategyTests : ContainerTestBase {
        private ShellDescriptor _descriptor;
        private IList<FeatureDescriptor> _features;
        private TestViewEngine _testViewEngine;


        protected override void Register(Autofac.ContainerBuilder builder) {
            _descriptor = new ShellDescriptor { };
            _testViewEngine = new TestViewEngine();

            builder.Register(ctx => _descriptor);
            builder.RegisterType<ShapeTemplateBindingStrategy>().As<IShapeTableProvider>();
            builder.RegisterType<BasicShapeTemplateHarvester>().As<IShapeTemplateHarvester>();
            builder.RegisterInstance(_testViewEngine).As<IShapeTemplateViewEngine>();

            var extensionManager = new Mock<IExtensionManager>();
            builder.Register(ctx => extensionManager);
            builder.Register(ctx => extensionManager.Object);
        }

        public class TestViewEngine : Dictionary<string, object>, IShapeTemplateViewEngine {
            public IEnumerable<string> DetectTemplateFileNames(string virtualPath) {
                var virtualPathNorm = virtualPath.Replace("\\", "/");

                foreach (var key in Keys) {
                    var keyNorm = key.Replace("\\", "/");

                    if (keyNorm.StartsWith(virtualPathNorm, StringComparison.OrdinalIgnoreCase)) {
                        var rest = keyNorm.Substring(virtualPathNorm.Length).TrimStart('/', '\\');
                        if (rest.IndexOfAny(new[] { '/', '\\' }) != -1) {
                            continue;
                        }
                        yield return Path.GetFileNameWithoutExtension(rest);
                    }
                }
            }
        }

        protected override void Resolve(IContainer container) {
            _features = new List<FeatureDescriptor>();

            container.Resolve<Mock<IExtensionManager>>()
                .Setup(em => em.AvailableFeatures())
                .Returns(_features);
        }

        void AddFeature(string name, params string[] dependencies) {
            var featureDescriptor = new FeatureDescriptor {
                Name = name,
                Dependencies = dependencies,
                Extension = new ExtensionDescriptor {
                    Name = name,
                    Location = "~/Modules"
                }
            };
            featureDescriptor.Extension.Features = new[] { featureDescriptor };

            _features.Add(featureDescriptor);
        }

        void AddEnabledFeature(string name, params string[] dependencies) {
            AddFeature(name, dependencies);
            _descriptor.Features = _descriptor.Features.Concat(new[] { new ShellFeature { Name = name } });
        }

        [Test]
        public void TemplateResolutionWorks() {
            AddEnabledFeature("Alpha");

            _testViewEngine.Add("~/Modules/Alpha/Views/AlphaShape.blah", null);
            var strategy = _container.Resolve<IShapeTableProvider>();

            IList<ShapeAlterationBuilder> alterationBuilders = new List<ShapeAlterationBuilder>();
            var builder = new ShapeTableBuilder(alterationBuilders,null);
            strategy.Discover(builder);
            var alterations = alterationBuilders.Select(alterationBuilder=>alterationBuilder.Build());

            Assert.That(alterations.Any(alteration => alteration.ShapeType == "AlphaShape"));
        }

    }
}

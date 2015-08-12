using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.ShellBuilders;
using Orchard.Tests.Environment.TestDependencies;
using Orchard.Utility.Extensions;

namespace Orchard.Tests.Environment.ShellBuilders {
    [TestFixture]
    public class CompositionStrategyTests : ContainerTestBase {
        private CompositionStrategy _compositionStrategy;
        private Mock<IExtensionManager> _extensionManager;

        protected override void Register(ContainerBuilder builder) {
            _extensionManager = new Mock<IExtensionManager>(MockBehavior.Loose);

            builder.RegisterType<CompositionStrategy>().AsSelf();
            builder.RegisterInstance(_extensionManager.Object);
        }

        protected override void Resolve(ILifetimeScope container) {
            _compositionStrategy = container.Resolve<CompositionStrategy>();

            var alphaExtension = new ExtensionDescriptor {
                Id = "Alpha",
                Name = "Alpha",
                ExtensionType = "Module"
            };

            var alphaFeatureDescriptor = new FeatureDescriptor {
                Id = "Alpha",
                Name = "Alpha",
                Extension = alphaExtension
            };

            var betaFeatureDescriptor = new FeatureDescriptor {
                Id = "Beta",
                Name = "Beta",
                Extension = alphaExtension,
                Dependencies = new List<string> {
                    "Alpha"
                }
            };

            alphaExtension.Features = new List<FeatureDescriptor> {
                alphaFeatureDescriptor,
                betaFeatureDescriptor
            };

            var features = new List<Feature> {
                new Feature {
                    Descriptor = alphaFeatureDescriptor,
                    ExportedTypes = new List<Type> {
                        typeof(AlphaDependency)
                    }
                },
                new Feature {
                    Descriptor = betaFeatureDescriptor,
                    ExportedTypes = new List<Type> {
                        typeof(BetaDependency)
                    }
                }
            };

            _extensionManager.Setup(x => x.AvailableExtensions()).Returns(new List<ExtensionDescriptor> {
                alphaExtension
            });

            _extensionManager.Setup(x => x.AvailableFeatures()).Returns(
                _extensionManager.Object.AvailableExtensions()
                .SelectMany(ext => ext.Features)
                .ToReadOnlyCollection());

            _extensionManager.Setup(x => x.LoadFeatures(It.IsAny<IEnumerable<FeatureDescriptor>>())).Returns(features);
        }

        [Test]
        public void ComposeReturnsBlueprintWithExpectedDependencies() {
            var shellSettings = CreateShell();
            var shellDescriptor = CreateShellDescriptor("Alpha", "Beta");
            var shellBlueprint = _compositionStrategy.Compose(shellSettings, shellDescriptor);

            Assert.That(shellBlueprint.Dependencies.Count(x => x.Type == typeof (AlphaDependency)), Is.EqualTo(1));
            Assert.That(shellBlueprint.Dependencies.Count(x => x.Type == typeof(BetaDependency)), Is.EqualTo(1));
        }

        [Test]
        public void ComposeReturnsBlueprintWithAutoEnabledDependencyFeatures() {
            var shellSettings = CreateShell();
            var shellDescriptor = CreateShellDescriptor("Beta"); // Beta has a dependency on Alpha, but is not enabled initially.
            var shellBlueprint = _compositionStrategy.Compose(shellSettings, shellDescriptor);

            Assert.That(shellBlueprint.Dependencies.Count(x => x.Type == typeof(AlphaDependency)), Is.EqualTo(1));
            Assert.That(shellDescriptor.Features.Count(x => x.Name == "Alpha"), Is.EqualTo(1));
        }

        private ShellSettings CreateShell() {
            return new ShellSettings();
        }

        private ShellDescriptor CreateShellDescriptor(params string[] enabledFeatures) {
            var shellDescriptor = new ShellDescriptor {
                Features = enabledFeatures.Select(x => new ShellFeature {
                    Name = x
                })
            };

            return shellDescriptor;
        }
    }
}

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
using Orchard.Logging;
using Orchard.Tests.Environment.TestDependencies;
using Orchard.Utility.Extensions;

namespace Orchard.Tests.Environment.ShellBuilders {
    [TestFixture]
    public class CompositionStrategyTests : ContainerTestBase {
        private CompositionStrategy _compositionStrategy;
        private Mock<IExtensionManager> _extensionManager;
        private IEnumerable<ExtensionDescriptor> _availableExtensions;
        private IEnumerable<Feature> _installedFeatures;
        private Mock<ILogger> _loggerMock;

        protected override void Register(ContainerBuilder builder) {
            _extensionManager = new Mock<IExtensionManager>();
            _loggerMock = new Mock<ILogger>();

            builder.RegisterType<CompositionStrategy>().AsSelf();
            builder.RegisterInstance(_extensionManager.Object);
            builder.RegisterInstance(_loggerMock.Object);
        }

        protected override void Resolve(ILifetimeScope container) {
            _compositionStrategy = container.Resolve<CompositionStrategy>();
            _compositionStrategy.Logger = container.Resolve<ILogger>();

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

            _availableExtensions = new[] {
                alphaExtension
            };

            _installedFeatures = new List<Feature> {
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

            _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            _extensionManager.Setup(x => x.AvailableExtensions()).Returns(() => _availableExtensions);

            _extensionManager.Setup(x => x.AvailableFeatures()).Returns(() =>
                _extensionManager.Object.AvailableExtensions()
                .SelectMany(ext => ext.Features)
                .ToReadOnlyCollection());

            _extensionManager.Setup(x => x.LoadFeatures(It.IsAny<IEnumerable<FeatureDescriptor>>())).Returns(() => _installedFeatures);
        }

        [Test]
        public void ComposeReturnsBlueprintWithExpectedDependencies() {
            var shellSettings = CreateShell();
            var shellDescriptor = CreateShellDescriptor("Alpha", "Beta");
            var shellBlueprint = _compositionStrategy.Compose(shellSettings, shellDescriptor);

            Assert.That(shellBlueprint.Dependencies.Count(x => x.Type == typeof(AlphaDependency)), Is.EqualTo(1));
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

        [Test]
        public void ComposeDoesNotThrowWhenFeatureStateRecordDoesNotExist() {
            var shellSettings = CreateShell();
            var shellDescriptor = CreateShellDescriptor("MyFeature");

            Assert.DoesNotThrow(() => _compositionStrategy.Compose(shellSettings, shellDescriptor));
            _loggerMock.Verify(x => x.Log(LogLevel.Warning, null, It.IsAny<string>(), It.IsAny<object[]>()));
        }

        [Test]
        public void ComposeThrowsWhenAutoEnabledDependencyDoesNotExist() {
            var myModule = _availableExtensions.First();

            myModule.Features = myModule.Features.Concat(new[] {
                new FeatureDescriptor {
                    Extension = myModule,
                    Name = "MyFeature",
                    Id = "MyFeature",
                    Dependencies = new[] { "NonExistingFeature" }
                }
            });
            
            var shellSettings = CreateShell();
            var shellDescriptor = CreateShellDescriptor("MyFeature");

            Assert.Throws<OrchardException>(() => _compositionStrategy.Compose(shellSettings, shellDescriptor));
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

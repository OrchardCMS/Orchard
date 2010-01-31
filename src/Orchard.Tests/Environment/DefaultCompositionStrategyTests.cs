using System.Linq;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Extensions;
using Orchard.Tests.ContentManagement.Records;
using Orchard.Tests.ContentManagement.Models;

namespace Orchard.Tests.Environment {
    [TestFixture]
    public class DefaultCompositionStrategyTests {
        [Test]
        public void ExpectedRecordsShouldComeBack() {
            var extensionManager = new Moq.Mock<IExtensionManager>();
            extensionManager.Setup(x => x.ActiveExtensions()).Returns(new[] {
                                                                            new ExtensionEntry
                                                                            {ExportedTypes = new[] {typeof (GammaRecord), typeof (DeltaRecord), typeof (Delta)}}
                                                                        });
            var strategy = new DefaultCompositionStrategy(extensionManager.Object);
            var recordTypes = strategy.GetRecordDescriptors();

            Assert.That(recordTypes.Count(), Is.Not.EqualTo(0));
            Assert.That(recordTypes, Has.Some.EqualTo(typeof(DeltaRecord)));
            Assert.That(recordTypes, Has.Some.EqualTo(typeof(GammaRecord)));
            Assert.That(recordTypes, Has.None.EqualTo(typeof(Delta)));
        }
    }
}

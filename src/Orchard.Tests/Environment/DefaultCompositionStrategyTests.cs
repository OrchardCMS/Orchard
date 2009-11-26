using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Packages;
using Orchard.Tests.Models.Records;
using Orchard.Tests.Models.Stubs;

namespace Orchard.Tests.Environment {
    [TestFixture]
    public class DefaultCompositionStrategyTests {
        [Test]
        public void ExpectedRecordsShouldComeBack() {
            var packageManager = new Moq.Mock<IPackageManager>();
            packageManager.Setup(x => x.ActivePackages()).Returns(new[] {
                                                                            new PackageEntry
                                                                            {ExportedTypes = new[] {typeof (GammaRecord), typeof (DeltaRecord), typeof (Delta)}}
                                                                        });
            var strategy = new DefaultCompositionStrategy(packageManager.Object);
            var recordTypes = strategy.GetRecordTypes();

            Assert.That(recordTypes.Count(), Is.Not.EqualTo(0));
            Assert.That(recordTypes, Has.Some.EqualTo(typeof(DeltaRecord)));
            Assert.That(recordTypes, Has.Some.EqualTo(typeof(GammaRecord)));
            Assert.That(recordTypes, Has.None.EqualTo(typeof(Delta)));
        }
    }
}

using System.Globalization;
using NUnit.Framework;
using Orchard.Media.Extensions;

namespace Orchard.Tests.Modules.Media.Extensions {
    [TestFixture]
    public class LongExtensionsTests {
        [Test]
        public void BytesAreFriendly() {
            long size = 123;
            string friendly = size.ToFriendlySizeString();
            Assert.That(friendly, Is.EqualTo("123 B"));
        }

        [Test]
        public void KilobytesAreFriendly() {
            long size = 93845;
            string friendly = size.ToFriendlySizeString();
            Assert.That(friendly, Is.EqualTo("92 KB"));
        }

        [Test]
        public void MegabytesAreFriendly() {
            long size = 6593528;
            string friendly = size.ToFriendlySizeString();

            Assert.That(friendly, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == "." ?
                Is.EqualTo("6.3 MB") :
                Is.EqualTo("6,3 MB"));
        }

        [Test]
        public void GigabytesAreFriendly() {
            long size = 46896534657;
            string friendly = size.ToFriendlySizeString();

            Assert.That(friendly, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == "." ? 
                Is.EqualTo("43.68 GB") : 
                Is.EqualTo("43,68 GB"));
        }

        [Test]
        public void TerabytesAreFriendly() {
            long size = 386594723458690;
            string friendly = size.ToFriendlySizeString();

            Assert.That(friendly, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == "." ?
                Is.EqualTo("351.606 TB") :
                Is.EqualTo("351,606 TB"));
        }

        [Test]
        public void PetabytesAreSlightlyFriendlyAsTerabytes() {
            long size = 56794738495678965;
            string friendly = size.ToFriendlySizeString();

            Assert.That(friendly, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == "." ?
                Is.EqualTo("51654.514 TB") :
                Is.EqualTo("51654,514 TB"));
        }

        [Test]
        public void VeryLargeSizeDoesNotCauseFailure() {
            long size = 5679473849567896593;
            string friendly = size.ToFriendlySizeString();

            Assert.That(friendly, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == "." ?
                Is.EqualTo("5165451.375 TB") :
                Is.EqualTo("5165451,375 TB"));
        }

        [Test]
        public void NegativeSizeDoesNotCauseFailure(){
            long size = -2598;
            string friendly = size.ToFriendlySizeString();
            Assert.That(friendly, Is.EqualTo("-2598 B"));
        }
    }
}
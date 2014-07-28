using System;
using NUnit.Framework;
using Orchard.Localization.Models;

namespace Orchard.Framework.Tests.Localization {

    [TestFixture]
    public class DateTimePartsTests {

        [Test]
        [Description("Equal instances return equality.")]
        public void EqualsTest01() {
            var target = new DateTimeParts(2014, 5, 31, 10, 0, 0, 0);
            var other = new DateTimeParts(2014, 5, 31, 10, 0, 0, 0);

            var result = target.Equals(other);

            Assert.IsTrue(result);
        }

        [Test]
        [Description("Different instances do not return equality.")]
        public void EqualsTest02() {
            var target = new DateTimeParts(2014, 5, 31, 10, 0, 0, 0);
            var other = new DateTimeParts(2014, 5, 31, 10, 0, 0, 1);

            var result = target.Equals(other);

            Assert.IsFalse(result);
        }
    }
}

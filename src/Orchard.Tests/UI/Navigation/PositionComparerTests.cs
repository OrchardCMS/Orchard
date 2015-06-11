using System;
using System.Collections.Generic;
using NUnit.Framework;
using Orchard.UI;

namespace Orchard.Tests.UI.Navigation {
    [TestFixture]
    public class PositionComparerTests {
        private IComparer<string> _comparer;

        [SetUp]
        public void Init() {
            _comparer = new FlatPositionComparer();
        }


        [Test]
        public void LessThanAndGreaterThanShouldBeBelowAndAboveZero() {
            var lessThan = StringComparer.InvariantCultureIgnoreCase.Compare("alpha", "beta");
            var greaterThan = StringComparer.InvariantCultureIgnoreCase.Compare("gamma", "delta");

            Assert.That(lessThan, Is.LessThan(0));
            Assert.That(greaterThan, Is.GreaterThan(0));
        }


        [Test]
        public void NullIsLessThanEmptyAndEmptyIsLessThanNonEmpty() {
            Assert.That(_comparer.Compare(null, ""), Is.LessThan(0));
            Assert.That(_comparer.Compare("", "5"), Is.LessThan(0));
            Assert.That(_comparer.Compare(null, "5"), Is.LessThan(0));

            Assert.That(_comparer.Compare("", null), Is.GreaterThan(0));
            Assert.That(_comparer.Compare("5", ""), Is.GreaterThan(0));
            Assert.That(_comparer.Compare("5", null), Is.GreaterThan(0));

            Assert.That(_comparer.Compare(null, null), Is.EqualTo(0));
            Assert.That(_comparer.Compare("", ""), Is.EqualTo(0));
        }

        [Test]
        public void NumericValuesShouldCompareNumerically() {
            AssertLess("3", "5");
            AssertMore("8", "5");
            AssertSame("5", "5");
            AssertMore("100", "5");
            AssertSame("007", "7");
        }

        [Test]
        public void NegativeNumericValuesAreLessThanPositive() {
            AssertLess("-5", "5");
            AssertSame("-5", "-5");
            AssertMore("42", "-42");
        }

        [Test]
        public void NegativeNumericValuesShouldCompareNumerically() {
            AssertMore("-3", "-5");
            AssertLess("-8", "-5");
            AssertSame("-5", "-5");
            AssertLess("-100", "-5");
            AssertSame("-007", "-7");
        }

        [Test]
        public void DotsSplitParts() {
            AssertLess("0500.3", "0500.5");
            AssertMore("0500.8", "0500.5");
            AssertSame("0500.5", "0500.5");
            AssertMore("0500.100", "0500.5");
            AssertSame("0500.007", "0500.7");

            AssertLess("0500.3.0300", "0500.5.0300");
            AssertMore("0500.8.0300", "0500.5.0300");
            AssertSame("0500.5.0300", "0500.5.0300");
            AssertMore("0500.100.0300", "0500.5.0300");
            AssertSame("0500.007.0300", "0500.7.0300");
        }

        [Test]
        public void NumericValuesShouldComeBeforeNonNumeric() {
            AssertLess("5", "x");
            AssertLess("50", "50a");
            AssertLess("1.50", "1.50a");
        }

        [Test]
        public void NonNumericValuesCompareOrdinallyAndIgnoreCase() {
            AssertSame("x", "X");
            AssertLess("rt675x", "rt685x");
            AssertMore("ru675x", "rt675x");
            AssertLess("rt675x", "rt675y");

            AssertSame("1.x.5", "1.X.5");
            AssertLess("1.rt675x.5", "1.rt685x.5");
            AssertMore("1.ru675x.5", "1.rt675x.5");
            AssertLess("1.rt675x.5", "1.rt675y.5");

            AssertSame("x.5", "X.5");
            AssertLess("rt675x.5", "rt685x.5");
            AssertMore("ru675x.5", "rt675x.5");
            AssertLess("rt675x.5", "rt675y.5");

            AssertSame("1.x", "1.X");
            AssertLess("1.rt675x", "1.rt685x");
            AssertMore("1.ru675x", "1.rt675x");
            AssertLess("1.rt675x", "1.rt675y");
        }

        [Test]
        public void LongerNonNumericShouldComeLater() {
            AssertLess("rt675x", "rt675xx");
        }


        [Test]
        public void EmptyBitsAreSafeAndShouldComeFirst() {
            AssertSame("1.2.3", "1.2.3");
            AssertSame(".1.2.3.", ".1.2.3.");
            AssertSame(".1..3.", ".1..3.");
            AssertLess("1..3", "1.2.3");
            AssertLess(".1..3.", ".1.2.3.");

            AssertSame("a.b.c", "a.b.c");
            AssertSame(".a.b.c.", ".a.b.c.");
            AssertSame(".a..c.", ".a..c.");
            AssertLess("a..c", "a.b.c");
            AssertLess(".a..c.", ".a.b.c.");
        }

        [Test]
        public void AdditionalNonEmptySegmentsShouldComeLater() {
            AssertLess("1.2", "1.2.3");
            AssertSame("1.2", "1.2.");

            AssertLess("a.b", "a.b.c");
            AssertSame("a.b", "a.b.");

        }

        void AssertLess(string x, string y) {
            Assert.That(_comparer.Compare(x, y), Is.LessThan(0));
            Assert.That(_comparer.Compare(y, x), Is.GreaterThan(0));
        }
        void AssertMore(string x, string y) {
            Assert.That(_comparer.Compare(x, y), Is.GreaterThan(0));
            Assert.That(_comparer.Compare(y, x), Is.LessThan(0));
        }
        void AssertSame(string x, string y) {
            Assert.That(_comparer.Compare(x, y), Is.EqualTo(0));
            Assert.That(_comparer.Compare(y, x), Is.EqualTo(0));
        }
    }
}

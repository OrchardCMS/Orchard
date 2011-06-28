using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Orchard.UI;

namespace Orchard.Tests.UI.Navigation {
    [TestFixture]
    public class PositionComparerTests {
        private IComparer<string> _comparer;

        [SetUp]
        public void Init() {
            _comparer = new PositionComparer();
        }


        [Test]
        public void LessThanAndGreaterThanShouldBeBelowAndAboveZero() {
            var lessThan = StringComparer.InvariantCultureIgnoreCase.Compare("alpha", "beta");
            var greaterThan = StringComparer.InvariantCultureIgnoreCase.Compare("gamma", "delta");

            Assert.That(lessThan, Is.LessThan(0));
            Assert.That(greaterThan, Is.GreaterThan(0));
        }

        [Test]
        public void AfterIsAlwaysAfter() {
            AssertMore("after", null);
            AssertMore("after", "");
            AssertMore("after", "0");
            AssertMore("after", "-1");
            AssertMore("after", "foo");
            AssertMore("after", int.MaxValue.ToString());
            AssertMore("after", "before");
            AssertSame("after", "after");

            AssertLess("after", "after.");
            AssertLess("after", "after.-10");
            AssertLess("after", "after.10");
        }

        [Test]
        public void BeforeIsAlways() {
            AssertLess("before", null);
            AssertLess("before", "");
            AssertLess("before", "0");
            AssertLess("before", "-1");
            AssertLess("before", "foo");
            AssertLess("before", int.MaxValue.ToString());
            AssertSame("before", "before");
            AssertLess("before", "after");

            AssertLess("before", "before.");
            AssertLess("before", "before.-10");
            AssertLess("before", "before.10");
        }
        [Test]
        public void NullIsLessThanEmptyAndEmptyIsLessThanNonEmpty() {
            Assert.That(_comparer.Compare(null, ""), Is.LessThan(0));
            Assert.That(_comparer.Compare("", "5"), Is.LessThan(0));
            Assert.That(_comparer.Compare(null, "5"), Is.LessThan(0));
            Assert.That(_comparer.Compare(null, "0"), Is.LessThan(0));

            Assert.That(_comparer.Compare("", null), Is.GreaterThan(0));
            Assert.That(_comparer.Compare("5", ""), Is.GreaterThan(0));
            Assert.That(_comparer.Compare("5", null), Is.GreaterThan(0));

            Assert.That(_comparer.Compare(null, null), Is.EqualTo(0));
            Assert.That(_comparer.Compare("", ""), Is.EqualTo(0));
            Assert.That(_comparer.Compare("", "0"), Is.LessThan(0));
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
            AssertLess("a", "c");
            AssertLess("1", "x");
            AssertLess("10", "x");
            AssertLess("10", "bar");
            AssertLess(int.MaxValue.ToString(), "bar");

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

            AssertSame("before", "Before");
            AssertSame("after", "After");
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
            AssertLess("1.2", "1.2.");

            AssertLess("a.b", "a.b.c");
            AssertLess("a.b", "a.b.");

            AssertLess("a.b", "a.b.-1");
            AssertLess("a.b", "a.b.before");
            AssertLess("a.b", "a.b.after");
        }

        [Test]
        public void AfterShouldReturnGreaterValues() {
            AssertMore(PositionComparer.After(null), null);
            AssertMore(PositionComparer.After(""), "");
            AssertMore(PositionComparer.After("-1"), "-1");
            AssertMore(PositionComparer.After("before"), "before");
            AssertSame(PositionComparer.After("after"), "after");
            AssertMore(PositionComparer.After("42"), "42");
            AssertMore(PositionComparer.After("foo"), "foo");
            AssertMore(PositionComparer.After("a.b"), "a.b");
            AssertMore(PositionComparer.After("1.2"), "1.2");
            AssertMore(PositionComparer.After("1."), "1.");
            AssertMore(PositionComparer.After("-0"), "-0");
        }

        [Test]
        public void BeforeShouldReturnLowerValues() {
            AssertLess(PositionComparer.Before(null), null);
            AssertLess(PositionComparer.Before(""), "");
            AssertLess(PositionComparer.Before("-1"), "-1");
            AssertSame(PositionComparer.Before("before"), "before");
            AssertLess(PositionComparer.Before("after"), "after");
            AssertLess(PositionComparer.Before("42"), "42");
            AssertLess(PositionComparer.Before("foo"), "foo");
            AssertLess(PositionComparer.Before("a.b"), "a.b");
            AssertLess(PositionComparer.Before("1.2"), "1.2");
            AssertLess(PositionComparer.Before("1."), "1.");
            AssertLess(PositionComparer.Before("-0"), "-0");
        }

        [Test]
        public void MaxShouldReturnTheGreaterValue() {
            Assert.That(PositionComparer.Max("1", "2"), Is.EqualTo("2"));
            Assert.That(PositionComparer.Max("a", "c"), Is.EqualTo("c"));
        }

        [Test]
        public void MinShouldReturnTheLowerValue() {
            Assert.That(PositionComparer.Min("1", "2"), Is.EqualTo("1"));
            Assert.That(PositionComparer.Min("a", "c"), Is.EqualTo("a"));
        }

        [Test]
        public void EqualValuesShouldReturnFirstParameter() {
            Assert.That(PositionComparer.Between("1.01", "1.001"), Is.EqualTo("1.01"));
            Assert.That(PositionComparer.Between("before", "before"), Is.EqualTo("before"));
            Assert.That(PositionComparer.Between("before.1", "before.1"), Is.EqualTo("before.1"));
        }

        [Test]
        public void ShouldComputeAverageValues() {
            Assert.That(PositionComparer.Between("1", "3"), Is.EqualTo("2"));
            Assert.That(PositionComparer.Between("-5", "0005"), Is.EqualTo("0"));

            AssertBetween("a", "c");
            AssertBetween("-10", "10");
            AssertBetween("foo", "bar");
            AssertBetween("-10", "bar");
            AssertBetween("10", "bar");
            AssertBetween("10", "before");
            AssertBetween("10", "after");
            AssertBetween("after", "before");

            // adjacent cases
            AssertBetween("1", "2");
            AssertBetween("1.1", "1.2");

            AssertBetween("ae", "bb");
        }

        [DebuggerStepThrough]
        void AssertLess(string x, string y) {
            Assert.That(_comparer.Compare(x, y), Is.LessThan(0));
            Assert.That(_comparer.Compare(y, x), Is.GreaterThan(0));
        }

        [DebuggerStepThrough]
        void AssertMore(string x, string y) {
            Assert.That(_comparer.Compare(x, y), Is.GreaterThan(0));
            Assert.That(_comparer.Compare(y, x), Is.LessThan(0));
        }

        [DebuggerStepThrough]
        void AssertSame(string x, string y) {
            Assert.That(_comparer.Compare(x, y), Is.EqualTo(0));
            Assert.That(_comparer.Compare(y, x), Is.EqualTo(0));
        }

        void AssertBetween(string x, string y) {
            var min = PositionComparer.Min(x, y);
            var max = PositionComparer.Max(x, y);
            var mean = PositionComparer.Between(x, y);

            AssertLess(mean, max);
            AssertMore(mean, min);
        }
    }
}

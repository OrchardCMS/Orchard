using System.Web.Routing;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Tests.UI.Navigation {
    [TestFixture]
    public class MenuItemComparerTests {
        [Test]
        public void TextShouldCauseDifferenceAndNullRouteValuesAreEqual() {
            var item1 = new MenuItem { Text = new LocalizedString("hello") };
            var item2 = new MenuItem { Text = new LocalizedString("hello") };
            var item3 = new MenuItem { Text = new LocalizedString("hello3") };
            AssertSameSameDifferent(item1, item2, item3);
        }

        [Test]
        public void NullRouteValuesShouldEqualEmptyRouteValues() {
            var item1 = new MenuItem { Text = new LocalizedString("hello") };
            var item2 = new MenuItem { Text = new LocalizedString("hello") };
            var item3 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary() };
            var item4 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary() };
            AssertSameSameSame(item1, item2, item3);
            AssertSameSameSame(item3, item4, item1);
        }
        [Test]
        public void AdditionalPropertiesShouldMismatch() {
            var item1 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = 1 }) };
            var item2 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = 1 }) };
            var item3 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = 1, two = 2 }) };
            AssertSameSameDifferent(item1, item2, item3);
        }

        [Test]
        public void ValueTypeShouldMismatch() {
            var item1 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = 1 }) };
            var item2 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = 1 }) };
            var item3 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = "1" }) };
            AssertSameSameDifferent(item1, item2, item3);
        }

        [Test]
        public void ValuesShouldMismatch() {
            var item1 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = "1", two = "2" }) };
            var item2 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = "1", two = "2" }) };
            var item3 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = "1", two = "3" }) };
            AssertSameSameDifferent(item1, item2, item3);
        }

        [Test]
        public void PositionAndChildrenDontMatter() {
            var item1 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = "1", two = "2" }) };
            var item2 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = "1", two = "2" }), Position = "4.0" };
            var item3 = new MenuItem { Text = new LocalizedString("hello"), RouteValues = new RouteValueDictionary(new { one = "1", two = "2" }), Items = new[] { new MenuItem() } };
            AssertSameSameSame(item1, item2, item3);
        }

        private static void AssertSameSameDifferent(MenuItem item1, MenuItem item2, MenuItem item3) {
            var comparer = new MenuItemComparer();

            Assert.That(comparer.Equals(item1, item2), Is.True);
            Assert.That(comparer.Equals(item1, item3), Is.False);
            Assert.That(comparer.Equals(item2, item3), Is.False);

            Assert.That(comparer.GetHashCode(item1), Is.EqualTo(comparer.GetHashCode(item2)));
            // - hash inequality isn't really guaranteed, now that you mention it
            //Assert.That(comparer.GetHashCode(item1), Is.Not.EqualTo(comparer.GetHashCode(item3)));
            //Assert.That(comparer.GetHashCode(item2), Is.Not.EqualTo(comparer.GetHashCode(item3)));
        }

        private static void AssertSameSameSame(MenuItem item1, MenuItem item2, MenuItem item3) {
            var comparer = new MenuItemComparer();

            Assert.That(comparer.Equals(item1, item2), Is.True);
            Assert.That(comparer.Equals(item1, item3), Is.True);
            Assert.That(comparer.Equals(item2, item3), Is.True);

            Assert.That(comparer.GetHashCode(item1), Is.EqualTo(comparer.GetHashCode(item2)));
            Assert.That(comparer.GetHashCode(item1), Is.EqualTo(comparer.GetHashCode(item3)));
            Assert.That(comparer.GetHashCode(item2), Is.EqualTo(comparer.GetHashCode(item3)));
        }
    }
}


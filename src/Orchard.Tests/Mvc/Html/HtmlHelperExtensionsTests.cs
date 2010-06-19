using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Orchard.Mvc.Html;
using System.Collections.Generic;

namespace Orchard.Tests.Mvc.Html {
    [TestFixture]
    public class HtmlHelperExtensionsTests {
        [Test]
        public void LinkReturnsIHtmlString() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.Link("test", "http://example.com") as IHtmlString;

            //assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void LinkHtmlEncodesLinkText() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.Link("<br />", "http://example.com");

            //assert
            Assert.AreEqual(@"<a href=""http://example.com"">&lt;br /&gt;</a>", result.ToString());
        }

        [Test]
        public void LinkHtmlAttributeEncodesHref() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.Link("test", "<br />");

            //assert
            Assert.AreEqual(@"<a href=""&lt;br />"">test</a>", result.ToString());
        }

        [Test]
        public void LinkHtmlAttributeEncodesAttributes() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.Link("linkText", "http://example.com", new { title = "<br />" });

            //assert
            Assert.AreEqual(@"<a href=""http://example.com"" title=""&lt;br />"">linkText</a>", result.ToString());
        }

        [Test]
        public void LinkOrDefaultReturnsIHtmlString() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.LinkOrDefault("test", "http://example.com") as IHtmlString;

            //assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void LinkOrDefaultHtmlEncodesLinkText() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.LinkOrDefault("<br />", "http://example.com");

            //assert
            Assert.AreEqual(@"<a href=""http://example.com"">&lt;br /&gt;</a>", result.ToString());
        }

        [Test]
        public void LinkOrDefaultWithoutHrefHtmlEncodesLinkText() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.LinkOrDefault("<br />", null);

            //assert
            Assert.AreEqual(@"&lt;br /&gt;", result.ToString());
        }

        [Test]
        public void LinkOrDefaultWithHrefHtmlAttributeEncodesHref() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.LinkOrDefault("test", "<br />");

            //assert
            Assert.AreEqual(@"<a href=""&lt;br />"">test</a>", result.ToString());
        }

        [Test]
        public void SelectOptionHtmlEncodesText() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.SelectOption("value", false, "<br />");

            //assert
            Assert.AreEqual(@"<option value=""value"">&lt;br /&gt;</option>", result.ToString());
        }

        [Test]
        public void UnorderedListWithNullItemsReturnsEmptyHtmlString() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.UnorderedList((IEnumerable<string>)null, (a, b) => MvcHtmlString.Create(""), "test");

            //assert
            Assert.AreEqual(string.Empty, result.ToString());
        }

        [Test]
        public void UnorderedListWithEmptyItemsReturnsEmptyHtmlString() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.UnorderedList(new string[] { }, (a, b) => MvcHtmlString.Create(""), "test");

            //assert
            Assert.AreEqual(string.Empty, result.ToString());
        }
    }
}

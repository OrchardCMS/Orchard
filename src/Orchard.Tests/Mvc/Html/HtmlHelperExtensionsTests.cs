using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
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

        [Test]
        public void HtmlHelperForEnablesLocalHelperMethods() {
            //arrange
            var controller = new FooController {
                ControllerContext = new ControllerContext()
            };
            var viewContext = new ViewContext {
                ViewData = new ViewDataDictionary {
                    TemplateInfo = new TemplateInfo {
                        HtmlFieldPrefix = "topprefix"
                    }
                },
                Controller = controller,
                View = new Mock<IView>().Object,
                TempData = new TempDataDictionary(),
                Writer = TextWriter.Null
            };
            var viewDataContainer = new Mock<IViewDataContainer>();
            viewDataContainer.SetupGet(o => o.ViewData).Returns(() => new ViewDataDictionary());
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);
            var localHelper = html.HtmlHelperFor(new {SomeString = "foo"}, "prefix");

            //act
            var result = localHelper.LabelFor(p => p.SomeString, "bar", null);

            //assert
            Assert.AreEqual(@"<label for=""prefix_SomeString"">bar</label>", result.ToString());
        }
        private class FooController : Controller { }

        [Test]
        public void Ellipsize_DontCutHtmlEncodedChars() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.Ellipsize("foo & bar", 5);

            //assert
            Assert.AreEqual("foo &amp;&#160;\u2026", result.ToString());

        }

        [Test]
        public void Excerpt_DontCutHtmlEncodedChars() {
            //arrange
            var viewContext = new ViewContext();
            var viewDataContainer = new Mock<IViewDataContainer>();
            var html = new HtmlHelper(viewContext, viewDataContainer.Object);

            //act
            var result = html.Excerpt("<p>foo &amp; bar</p>", 7);

            //assert
            Assert.AreEqual("foo &amp;&#160;\u2026", result.ToString());

        }
    }
}

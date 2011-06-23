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
    }
}

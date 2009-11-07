using System.Web.Mvc;
using NUnit.Framework;
using Orchard.Web.Controllers;

namespace Orchard.Web.Tests.Controllers {
    [TestFixture]
    public class HomeControllerTests {
        [Test]
        public void IndexAndAboutShouldReturnViews() {
            var controller = new HomeController();
            var result1 = controller.Index();
            var result2 = controller.About();

            Assert.That(result1, Is.TypeOf<ViewResult>());
            Assert.That(result2, Is.TypeOf<ViewResult>());
        }
    }
}
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.Mvc.ViewModels;
using Orchard.Tests.Stubs;
using Orchard.UI.Notify;

namespace Orchard.Tests.UI.Notify {
    [TestFixture]
    public class NotifyFilterTests {
        private static ActionExecutedContext BuildContext() {
            var httpContext = new StubHttpContext();
            var routeData = new RouteData();

            var controllerContext = new ControllerContext(httpContext, routeData, new Mock<ControllerBase>().Object);
            var actionDescriptor = new Mock<ActionDescriptor>().Object;
            return new ActionExecutedContext(controllerContext, actionDescriptor, false/*cancelled*/, null/*exception*/);
        }

        [Test]
        public void AfterActionExecutedMessagesShouldAppearInTempData() {
            var sink = new Notifier();
            var filter = new NotifyFilter(sink);
            sink.Information("Hello world");

            var executedContext = BuildContext();
            filter.OnActionExecuted(executedContext);

            Assert.That(executedContext.Controller.TempData.ContainsKey("messages"));
            Assert.That(executedContext.Controller.TempData["messages"], Is.StringContaining("Hello world"));
        }

        [Test]
        public void ExistingTempDataIsntModified() {
            var sink = new Notifier();
            var filter = new NotifyFilter(sink);

            var executedContext = BuildContext();
            executedContext.Controller.TempData.Add("messages", "dont-destroy");
            filter.OnActionExecuted(executedContext);
            Assert.That(executedContext.Controller.TempData["messages"], Is.EqualTo("dont-destroy"));
        }

        [Test]
        public void NewMessagesAreConcatinated() {
            var sink = new Notifier();
            var filter = new NotifyFilter(sink);
            sink.Error("Boom");

            var executedContext = BuildContext();
            executedContext.Controller.TempData.Add("messages", "dont-destroy");
            filter.OnActionExecuted(executedContext);
            Assert.That(executedContext.Controller.TempData["messages"], Is.StringContaining("dont-destroy"));
            Assert.That(executedContext.Controller.TempData["messages"], Is.StringContaining("dont-destroy"));
        }

        [Test]
        public void TempDataBuildsMessagesWhenResultExecutingIsBaseViewModel() {
            var sink = new Notifier();
            var filter = new NotifyFilter(sink);
            sink.Information("Working");

            var model = new AdminViewModel();

            var context = BuildContext();
            context.Controller.TempData.Add("messages", "dont-destroy" + System.Environment.NewLine);
            context.Result = new ViewResult {
                                                ViewData = new ViewDataDictionary<AdminViewModel>(model),
                                                TempData = context.Controller.TempData
                                            };

            filter.OnActionExecuted(context);
            filter.OnResultExecuting(new ResultExecutingContext(context, context.Result));

            var T = NullLocalizer.Instance;

            Assert.That(model.Messages, Is.Not.Null);
            Assert.That(model.Messages, Has.Count.EqualTo(2));
            Assert.That(model.Messages, Has.Some.Property("Message").EqualTo(T("dont-destroy")));
            Assert.That(model.Messages, Has.Some.Property("Message").EqualTo(T("Working")));
        }
    }
}
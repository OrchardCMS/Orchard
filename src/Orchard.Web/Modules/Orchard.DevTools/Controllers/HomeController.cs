using System;
using System.Dynamic;
using System.Web.Mvc;
using Orchard.DevTools.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.ViewModels;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.UI.Admin;

namespace Orchard.DevTools.Controllers {
    [Themed]
    [Admin]
    public class HomeController : Controller {
        private readonly INotifier _notifier;

        public HomeController(INotifier notifier, IShapeHelperFactory shapeHelperFactory) {
            _notifier = notifier;
            T = NullLocalizer.Instance;
            New = shapeHelperFactory.CreateShapeHelper();
        }

        dynamic New { get; set; }

        public Localizer T { get; set; }

        public ActionResult Index() {
            return View(new BaseViewModel());
        }

        public ActionResult NotAuthorized() {
            _notifier.Warning(T("Simulated error goes here."));
            return new HttpUnauthorizedResult();
        }

        public ActionResult Simple() {
            return View(new Simple { Title = "This is a simple text", Quantity = 5 });
        }

        public ActionResult _RenderableAction() {
            return PartialView("_RenderableAction", "This is render action");
        }

        public ActionResult SimpleMessage() {
            _notifier.Information(T("Notifier works without BaseViewModel"));
            return RedirectToAction("Simple");
        }

        [Themed(false)]
        public ActionResult SimpleNoTheme() {
            return View("Simple", new Simple { Title = "This is not themed", Quantity = 5 });
        }

        public ActionResult UsingShapes() {
            ViewModel.Page = New.Page()
                .Main(New.Zone(typeof(Array), Name: "Main"))
                .Messages(New.Zone(typeof(Array), Name: "Main"))
                .Sidebar(New.Zone(typeof(Array), Name: "Main"));

            ViewModel.Page.Messages.Add(
                New.Message(Content: T("This is a test"), Severity: "Really bad!!!"));

            var model = New.Explosion(Height: 100, Width: 200);

            return View("UsingShapes", model);
        }

        public static string Break(dynamic view) {
            return view.Model.Box.Title;
        }
    }
    public class MyViewModel {
        public dynamic Box { get; set; }
    }
}

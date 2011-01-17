using System;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Experimental.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.UI.Admin;

namespace Orchard.Experimental.Controllers {

    [Themed, Admin]
    public class HomeController : Controller {
        private readonly INotifier _notifier;
        private readonly IContainerSpyOutput _containerSpyOutput;

        public HomeController(INotifier notifier, IShapeFactory shapeFactory, IContainerSpyOutput containerSpyOutput) {
            _notifier = notifier;
            _containerSpyOutput = containerSpyOutput;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        public Localizer T { get; set; }

        public ActionResult Index() {
            return View();
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

        public ActionResult FormShapes() {
            var model = Shape.Form()
                .Fieldsets(Shape.Fieldsets(typeof(Array))
                    .Add(Shape.Fieldset(typeof(Array)).Name("site")
                        .Add(Shape.InputText().Name("SiteName").Text(T("Site Name")).Value(T("some default/pre-pop value...").Text))
                    )
                    .Add(Shape.Fieldset(typeof(Array)).Name("admin")
                        .Add(Shape.InputText().Name("AdminUsername").Text(T("Admin Username")))
                        .Add(Shape.InputPassword().Name("AdminPassword").Text(T("Admin Password")))
                    )
                    .Add(Shape.Fieldset(typeof(Array)).Name("actions")
                        .Add(Shape.FormSubmit().Text(T("Finish Setup")))
                    )
                );

            // get at the first input?
            model.Fieldsets[0][0].Attributes(new {autofocus = "autofocus"}); // <-- could be applied by some other behavior - need to be able to modify attributes instead of clobbering them like this

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("FormShapes")]
        public ActionResult FormShapesPOST() {
            //all reqs are fail
            ModelState.AddModelError("AdminUsername", "The admin username is wrong.");
            return FormShapes();
        }

        public ActionResult UsingShapes() {

            ViewBag.Page = Shape.Page()
                .Main(Shape.Zone(typeof (Array), Name: "Main"))
                .Messages(Shape.Zone(typeof (Array), Name: "Messages"))
                .Sidebar(Shape.Zone(typeof (Array), Name: "Sidebar"));

            //ViewModel.Page.Add("Messages:5", New.Message(Content: T("This is a test"), Severity: "Really bad!!!"));

            ViewBag.Page.Messages.Add(
                Shape.Message(Content: T("This is a test"), Severity: "Really bad!!!"));

            ViewBag.Page.Sidebar.Add(
                Shape.Link(Url: "http://orchard.codeplex.com", Content: Shape.Image(Url: "http://orchardproject.net/Content/images/orchardLogo.jpg").Attributes(new { @class = "bigredborderfromabadclassname" })));

            var model = Shape.Message(
                Content: Shape.Explosion(Height: 100, Width: 200),
                Severity: "Meh");

            ViewBag.Page.Messages.Add(new HtmlString("<hr/>abuse<hr/>"));
            ViewBag.Page.Messages.Add("<hr/>encoded<hr/>");

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        public static string Break(dynamic view) {
            return view.Model.Box.Title;
        }

        public ActionResult ContainerData() {
            var root = new XElement("root");
            _containerSpyOutput.Write(root);
            return Content(root.ToString(), "text/xml");
        }
    }

}

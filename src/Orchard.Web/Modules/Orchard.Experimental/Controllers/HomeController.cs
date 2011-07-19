using System;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.DisplayManagement;
using Orchard.Events;
using Orchard.Experimental.Models;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Themes;
using Orchard.Tokens;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Experimental.Controllers {
    public interface IFormEvents : IEventHandler {
        void Alter(dynamic form);
    }

    public class CaptchaRegisterFormEvents : IFormEvents {
        private readonly dynamic Shape;

        public CaptchaRegisterFormEvents(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Alter(dynamic form) {
            if (form.Id == "Register") {
                form._Captcha = Shape.Fieldset(
                    Title: T("Captcha"),
                    _Challenge: Shape.Image(
                        Title: T("Captcha"),
                        Src: "http://kjh-ptsa.org/drupal-7.2/sites/default/files/Logo.jpg"),
                    _Response: Shape.Textbox(
                        Id: "captcharesponse", Name: "captcharesponse",
                        Title: T("Captcha Response"), TitleDisplay: "attribute",
                        Description: T("Type what you see to prove you are a human")));
            }
        }
    }

    [Themed, Admin]
    public class HomeController : Controller {
        private readonly INotifier _notifier;
        private readonly IContainerSpyOutput _containerSpyOutput;
        private readonly IFormEvents _formEvents;
        private readonly ITokenManager _tokenManager;
        private readonly ITokenizer _tokenizer;
        private readonly IOrchardServices _orchardServices;

        public HomeController(INotifier notifier, IShapeFactory shapeFactory, IContainerSpyOutput containerSpyOutput, IFormEvents formEvents, ITokenManager tokenManager, ITokenizer tokenizer, IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            _notifier = notifier;
            _containerSpyOutput = containerSpyOutput;
            _formEvents = formEvents;
            _tokenManager = tokenManager;
            _tokenizer = tokenizer;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }

        public Localizer T { get; set; }

        public ActionResult Index() {
            return View();
        }

        public ActionResult NotAuthorized() {
            _notifier.Warning(T("Simulated error goes here."));
            return new HttpUnauthorizedResult();
        }

        public ActionResult Simple() {
            return View(new Simple {Title = "This is a simple text", Quantity = 5});
        }

        public ActionResult TestingForms() {
            var form = Shape.Form(
                Id: "Register",
                _Name: Shape.Textbox(
                    Id: "name", Name: "name",
                    Title: T("Username"),
                    Description: T("Spaces are allowed; punctuation is not allowed except for periods, hyphens, apostrophes, and underscores.")),
                _Mail: Shape.Textbox(
                    Id: "name", Name: "mail",
                    Title: T("E-mail address"),
                    Description: T("A valid e-mail address. All e-mails from the system will be sent to this address. The e-mail address is not made public and will only be used if you wish to receive a new password or wish to receive certain news or notifications by e-mail.")),
                _Actions: Shape.Fieldset(
                    _Ok: Shape.Submit(
                        Name: "op",
                        Value: T("Register"))));

            form._Actions.Metadata.Position = "10";

            _formEvents.Alter(form);

            form._Captcha._Response.Value = T("It's a pony!");

            return new ShapeResult(this, form);
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
            return View("Simple", new Simple {Title = "This is not themed", Quantity = 5});
        }

        public ActionResult FormShapes() {
            var model = Shape.Form()
                .Fieldsets(Shape.Fieldsets(typeof (Array))
                               .Add(Shape.Fieldset(typeof (Array)).Name("site")
                                        .Add(Shape.InputText().Name("SiteName").Text(T("Site Name")).Value(T("some default/pre-pop value...").Text))
                               )
                               .Add(Shape.Fieldset(typeof (Array)).Name("admin")
                                        .Add(Shape.InputText().Name("AdminUsername").Text(T("Admin Username")))
                                        .Add(Shape.InputPassword().Name("AdminPassword").Text(T("Admin Password")))
                               )
                               .Add(Shape.Fieldset(typeof (Array)).Name("actions")
                                        .Add(Shape.FormSubmit().Text(T("Finish Setup")))
                               )
                );

            // get at the first input?
            model.Fieldsets[0][0].Attributes(new {autofocus = "autofocus"}); // <-- could be applied by some other behavior - need to be able to modify attributes instead of clobbering them like this

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
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
                Shape.Link(Url: "http://orchard.codeplex.com", Content: Shape.Image(Url: "http://orchardproject.net/Content/images/orchardLogo.jpg").Attributes(new {@class = "bigredborderfromabadclassname"})));

            var model = Shape.Message(
                Content: Shape.Explosion(Height: 100, Width: 200),
                Severity: "Meh");

            ViewBag.Page.Messages.Add(new HtmlString("<hr/>abuse<hr/>"));
            ViewBag.Page.Messages.Add("<hr/>encoded<hr/>");

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        public static string Break(dynamic view) {
            return view.Model.Box.Title;
        }

        public ActionResult ContainerData() {
            var root = new XElement("root");
            _containerSpyOutput.Write(root);
            return Content(root.ToString(), "text/xml");
        }

        public ActionResult UseTokens() {
            var s = "";
            var input = @"Hello {Site.GlobalToken1} Hello {Site.GlobalToken2}{User.Name}{{escaped brackets}}";
            var replacement = _tokenizer.Replace(input, new { UserX = _orchardServices.WorkContext.CurrentUser });
            foreach (var tokenContext in _tokenizer.ParseTokens(input, new { UserX = _orchardServices.WorkContext.CurrentUser })) {
                s += string.Format("<li>Token '{0}' = [{1},{2}] {3}</li>", tokenContext.Token == null ? "n/a" : tokenContext.Token.Name, tokenContext.Offset, tokenContext.Length, tokenContext.Replacement);
            }
            return Content(input + "<hr/>" + replacement + "<hr/><ol>" + s + "</ol>");
            //var globalTokens = _tokenManager.GetTokenSet(null);
            //globalTokens.SetToken("GlobalToken3", "overridden");
            //return Content(string.Format("GlobalToken1={0}, GlobalToken2={1}, DoesNotExist={2}, GlobalToken3={3}",
            //    globalTokens.GetToken("GlobalToken1").Value,
            //    globalTokens.GetToken("GlobalToken2").Value,
            //    globalTokens.GetToken("DoesNotExist") == null ? null : globalTokens.GetToken("DoesNotExist").Value,
            //    globalTokens.GetToken("GlobalToken3").Value));
        }
    }


    public class ExperimentalTokenProvider : ITokenProvider {
        public ExperimentalTokenProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void BuildTokens(TokenBuilder builder) {
            builder.Describe("Site", "GlobalToken1", c => "[global1]");
            builder.Describe("Site", "GlobalToken2", c => "[global2]");
            builder.Describe<IUser>("User", "Name", u => u.UserName);
        }
    }
}
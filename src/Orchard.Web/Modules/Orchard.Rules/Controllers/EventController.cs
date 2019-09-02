using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Rules.Models;
using Orchard.Rules.Services;
using Orchard.Rules.ViewModels;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Rules.Controllers {
    [ValidateInput(false), Admin]
    public class EventController : Controller {
        public EventController(
            IOrchardServices services,
            IRulesManager rulesManager,
            IRulesServices rulesServices,
            IFormManager formManager,
            IShapeFactory shapeFactory) {
            Services = services;
            _rulesManager = rulesManager;
            _rulesServices = rulesServices;
            _formManager = formManager;
            Shape = shapeFactory;
        }

        public IOrchardServices Services { get; set; }
        private readonly IRulesManager _rulesManager;
        private readonly IRulesServices _rulesServices;
        private readonly IFormManager _formManager;
        public Localizer T { get; set; }
        public dynamic Shape { get; set; }

        public ActionResult Add(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            var viewModel = new AddEventViewModel { Id = id, Events = _rulesManager.DescribeEvents() };
            return View(viewModel);
        }

        public ActionResult Delete(int id, int eventId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            _rulesServices.DeleteEvent(eventId);
            Services.Notifier.Success(T("Event Deleted"));

            return RedirectToAction("Edit", "Admin", new { id });
        }

        public ActionResult Edit(int id, string category, string type, int eventId = -1) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            var ev = _rulesManager.DescribeEvents().SelectMany(x => x.Descriptors).Where(x => x.Category == category && x.Type == type).FirstOrDefault();

            if (ev == null) {
                return HttpNotFound();
            }

            // if there is no form to edit, save the action and go back to the rule
            if (ev.Form == null) {
                if (eventId == -1) {
                    var rule = _rulesServices.GetRule(id);
                    rule.Events.Add(new EventRecord { Category = category, Type = type });
                }

                return RedirectToAction("Edit", "Admin", new { id });
            }

            // build the form, and let external components alter it
            var form = _formManager.Build(ev.Form);

            // generate an anti forgery token
            AddSubmitButton(form);

            // bind form with existing values).
            if (eventId != -1) {
                var rule = _rulesServices.GetRule(id);
                var eventRecord = rule.Events.Where(a => a.Id == eventId).FirstOrDefault();
                if (eventRecord != null) {
                    var parameters = FormParametersHelper.FromString(eventRecord.Parameters);
                    _formManager.Bind(form, new DictionaryValueProvider<string>(parameters, CultureInfo.InvariantCulture));
                }
            }

            var viewModel = new EditEventViewModel { Id = id, Event = ev, Form = form };
            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id, string category, string type, [DefaultValue(-1)] int eventId, FormCollection formCollection) {
            var rule = _rulesServices.GetRule(id);

            var eventRecord = rule.Events.Where(a => a.Id == eventId).FirstOrDefault();

            // add new event record if it's a newly created event
            if (eventRecord == null) {
                eventRecord = new EventRecord { Category = category, Type = type };
                rule.Events.Add(eventRecord);
            }

            var e = _rulesManager.DescribeEvents().SelectMany(x => x.Descriptors).Where(x => x.Category == category && x.Type == type).FirstOrDefault();

            // validating form values
            _formManager.Validate(new ValidatingContext { FormName = e.Form, ModelState = ModelState, ValueProvider = ValueProvider });

            if (ModelState.IsValid) {
                var dictionary = formCollection.AllKeys.ToDictionary(key => key, formCollection.Get);

                // save form parameters
                eventRecord.Parameters = FormParametersHelper.ToString(dictionary);

                return RedirectToAction("Edit", "Admin", new { id });
            }

            // model is invalid, display it again
            var form = _formManager.Build(e.Form);
            AddSubmitButton(form);
            _formManager.Bind(form, formCollection);
            var viewModel = new EditEventViewModel { Id = id, Event = e, Form = form };

            return View(viewModel);
        }

        private void AddSubmitButton(dynamic form) {
            var viewContext = new ViewContext { HttpContext = HttpContext, Controller = this };
            var token = new HtmlHelper(viewContext, new ViewDataContainer()).AntiForgeryToken();

            // add a submit button to the form
            form
                ._Actions(Shape.Fieldset(
                    _RequestAntiForgeryToken: Shape.Markup(
                        Value: token.ToString()),
                    _Save: Shape.Submit(
                        Name: "op",
                        Value: T("Save"))
                    )
                );
        }

        private class ViewDataContainer : IViewDataContainer {
            public ViewDataDictionary ViewData { get; set; }
        }
    }
}
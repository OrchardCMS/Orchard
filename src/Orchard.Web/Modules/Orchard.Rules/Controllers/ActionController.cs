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
    public class ActionController : Controller {
        public ActionController(
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

            var viewModel = new AddActionViewModel { Id = id, Actions = _rulesManager.DescribeActions() };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Delete(int id, int actionId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            _rulesServices.DeleteAction(actionId);
            Services.Notifier.Information(T("Action Deleted"));

            return RedirectToAction("Edit", "Admin", new { id });
        }

        public ActionResult Edit(int id, string category, string type, int actionId = -1) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            var action = _rulesManager.DescribeActions().SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == category && x.Type == type);

            if (action == null) {
                return HttpNotFound();
            }

            // if there is no form to edit, save the action and go back to the rule
            if (action.Form == null) {
                if (actionId == -1) {
                    var rule = _rulesServices.GetRule(id);
                    rule.Actions.Add(new ActionRecord { Category = category, Type = type, Position = rule.Actions.Count + 1 });
                }

                return RedirectToAction("Edit", "Admin", new { id });
            }

            // build the form, and let external components alter it
            var form = _formManager.Build(action.Form);

            // generate an anti forgery token
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

            // bind form with existing values).
            if (actionId != -1) {
                var rule = _rulesServices.GetRule(id);
                var actionRecord = rule.Actions.FirstOrDefault(a => a.Id == actionId);
                if (actionRecord != null) {
                    var parameters = FormParametersHelper.FromString(actionRecord.Parameters);
                    _formManager.Bind(form, new DictionaryValueProvider<string>(parameters, CultureInfo.InvariantCulture));
                }
            }

            var viewModel = new EditActionViewModel { Id = id, Action = action, Form = form };
            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id, string category, string type, [DefaultValue(-1)]int actionId, FormCollection formCollection) {
            var rule = _rulesServices.GetRule(id);

            var actionRecord = rule.Actions.FirstOrDefault(a => a.Id == actionId);

            // add new action record if it's a newly created action
            if (actionRecord == null) {
                actionRecord = new ActionRecord { Category = category, Type = type, Position = rule.Actions.Count };
                rule.Actions.Add(actionRecord);
            }

            var action = _rulesManager.DescribeActions().SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == category && x.Type == type);

            // validating form values
            _formManager.Validate(new ValidatingContext { FormName = action.Form, ModelState = ModelState, ValueProvider = ValueProvider });

            if (ModelState.IsValid) {
                var dictionary = formCollection.AllKeys.ToDictionary(key => key, formCollection.Get);

                // save form parameters
                actionRecord.Parameters = FormParametersHelper.ToString(dictionary);

                return RedirectToAction("Edit", "Admin", new { id });
            }

            // model is invalid, display it again
            var form = _formManager.Build(action.Form);

            // Cancel the current transaction to prevent records from begin created
            Services.TransactionManager.Cancel();

            AddSubmitButton(form);

            _formManager.Bind(form, formCollection);
            var viewModel = new EditActionViewModel { Id = id, Action = action, Form = form };

            return View(viewModel);
        }

        private class ViewDataContainer : IViewDataContainer {
            public ViewDataDictionary ViewData { get; set; }
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
    }
}
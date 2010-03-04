using System;
using System.Web.Mvc;
using Futures.Widgets.Models;
using Futures.Widgets.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Settings;

namespace Futures.Widgets.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        public AdminController(IOrchardServices services) {
            Services = services;
        }

        private IOrchardServices Services { get; set; }
        protected virtual ISite CurrentSite { get; set; }

        public ActionResult AddWidget() {
            var hasWidgetsRecord = CurrentSite.As<HasWidgets>().Record;

            var widget = Services.ContentManager.Create<Widget>("HtmlWidget", init => {
                init.Record.Scope = hasWidgetsRecord;
                init.Record.Zone = "content";
                init.Record.Position = "after";
                init.As<BodyAspect>().Text = "Hello world!";
            });

            return RedirectToAction("Edit", new {widget.ContentItem.Id });
        }

        public ActionResult Edit(int id, string returnUrl) {
            var widget = Services.ContentManager.Get(id);
            var viewModel = new WidgetEditViewModel {
                Widget = Services.ContentManager.BuildEditorModel(widget),
                ReturnUrl = returnUrl,
            };
            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(int id, string returnUrl) {
            var widget = Services.ContentManager.Get(id);
            var viewModel = new WidgetEditViewModel {
                Widget = Services.ContentManager.UpdateEditorModel(widget, this),
                ReturnUrl = returnUrl,
            };
            if (ModelState.IsValid == false) {
                return View(viewModel);
            }
            if (string.IsNullOrEmpty(returnUrl)) {
                return RedirectToAction("Edit", new { id });
            }
            return Redirect(returnUrl);
        }


        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Futures.Widgets.Models;
using Futures.Widgets.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Services;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Futures.Widgets.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        public AdminController(IOrchardServices services, IEnumerable<IHtmlFilter> htmlFilters) {
            _htmlFilters = htmlFilters;
            Services = services;
            T = NullLocalizer.Instance;
        }

        private IOrchardServices Services { get; set; }
        protected virtual ISite CurrentSite { get; set; }
        public Localizer T{ get; set;}

        public ActionResult AddWidget(string zoneName, string themeName, string returnUrl) {
            var hasWidgetsRecord = CurrentSite.As<WidgetsPart>().Record;

            var virtualPath = "~/Themes/" + themeName + "/Zones/" + zoneName + ".html";
            var physicalPath = Server.MapPath(virtualPath);

            if (!System.IO.File.Exists(physicalPath)) {
                Services.Notifier.Error(T("Designer notes not found."));
                return Redirect(returnUrl);
            }

            var fileText = _htmlFilters
                .Aggregate(System.IO.File.ReadAllText(physicalPath), (text, filter) => filter.ProcessContent(text));

            var widget = Services.ContentManager.Create<WidgetPart>("HtmlWidget", init => {
                init.Record.Scope = hasWidgetsRecord;
                init.Record.Zone = zoneName;
                init.Record.Position = "1";
                init.As<BodyPart>().Text = fileText;
            });

            return RedirectToAction("Edit", new { widget.ContentItem.Id, returnUrl });
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

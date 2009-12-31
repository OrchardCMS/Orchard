using System.Linq;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Sandbox.Models;
using Orchard.Sandbox.ViewModels;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Orchard.Sandbox.Controllers {
    public class PageController : Controller, IUpdateModel {

        public PageController(IOrchardServices orchardServices) {
            Services = orchardServices;
        }

        public IOrchardServices Services { get; set; }
        public virtual ISite CurrentSite { get; set; }
        public virtual IUser CurrentUser { get; set; }
        public Localizer T { get; set; }


        public ActionResult Index() {
            var model = new PageIndexViewModel {
                Pages = Services.ContentManager.Query<SandboxPage, SandboxPageRecord>()
                    .OrderBy(x => x.Name)
                    .List()
                    .Select(x => Services.ContentManager.BuildDisplayModel(x, "SummaryList"))
            };
            return View(model);
        }

        public ActionResult Show(int id) {
            return View(new PageShowViewModel {
                Page = Services.ContentManager.BuildDisplayModel<SandboxPage>(id, "Detail")
            });
        }

        public ActionResult Create() {
            var settings = CurrentSite.Get<ContentPart<SandboxSettingsRecord>>();
            if (settings.Record.AllowAnonymousEdits == false && CurrentUser == null) {
                Services.Notifier.Error(T("Anonymous users can not create pages"));
                return RedirectToAction("index");
            }

            return View(new PageCreateViewModel());
        }


        [HttpPost]
        public ActionResult Create(PageCreateViewModel model) {
            var settings = CurrentSite.Get<ContentPart<SandboxSettingsRecord>>();
            if (settings.Record.AllowAnonymousEdits == false && CurrentUser == null) {
                Services.Notifier.Error(T("Anonymous users can not create pages"));
                return RedirectToAction("index");
            }

            var page = Services.ContentManager.Create<SandboxPage>("sandboxpage", item => {
                item.Record.Name = model.Name;
            });
            return RedirectToAction("show", new { page.ContentItem.Id });
        }


        public ActionResult Edit(int id) {
            if (IsEditAllowed() == false) {
                return RedirectToAction("show", new { id });
            }

            var latest = Services.ContentManager.GetLatest<SandboxPage>(id);
            return View(new PageEditViewModel {
                Page = Services.ContentManager.BuildEditorModel(latest)
            });
        }

        [HttpPost, ActionName("Edit"), ValidateInput(false)]
        public ActionResult _Edit(int id) {
            if (IsEditAllowed() == false) {
                return RedirectToAction("show", new { id });
            }
            var latest = Services.ContentManager.GetDraftRequired<SandboxPage>(id);
            var model = new PageEditViewModel {
                Page = Services.ContentManager.UpdateEditorModel(latest, this)
            };
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }
            Services.ContentManager.Publish(latest.ContentItem);
            return RedirectToAction("show", new { id });
        }

        bool IsEditAllowed() {
            var settings = CurrentSite.Get<ContentPart<SandboxSettingsRecord>>();
            if (settings.Record.AllowAnonymousEdits == false && CurrentUser == null) {
                Services.Notifier.Error(T("Anonymous users can not edit pages"));
                return false;
            }
            return true;
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}

using System;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Sandbox.Models;
using Orchard.Sandbox.ViewModels;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Orchard.Sandbox.Controllers {
    public class PageController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;

        public PageController(IContentManager contentManager, INotifier notifier) {
            _contentManager = contentManager;
            _notifier = notifier;
        }

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }
        protected virtual IUser CurrentUser { get; [UsedImplicitly] private set; }
        public Localizer T { get; set; }


        public ActionResult Index() {
            var model = new PageIndexViewModel {
                Pages = _contentManager.Query<SandboxPage, SandboxPageRecord>()
                    .OrderBy(x => x.Name)
                    .List()
                    .Select(x => _contentManager.BuildDisplayModel(x, "SummaryList"))
            };
            return View(model);
        }

        public ActionResult Show(int id) {
            var page = _contentManager.Get<SandboxPage>(id);
            var model = new PageShowViewModel {
                Page = _contentManager.BuildDisplayModel(page, "Detail")
            };
            return View(model);
        }

        public ActionResult Create() {
            var settings = CurrentSite.Get<ContentPart<SandboxSettingsRecord>>();
            if (settings.Record.AllowAnonymousEdits == false && CurrentUser == null) {
                _notifier.Error(T("Anonymous users can not create pages"));
                return RedirectToAction("index");
            }

            return View(new PageCreateViewModel());
        }


        [HttpPost]
        public ActionResult Create(PageCreateViewModel model) {
            var settings = CurrentSite.Get<ContentPart<SandboxSettingsRecord>>();
            if (settings.Record.AllowAnonymousEdits == false && CurrentUser == null) {
                _notifier.Error(T("Anonymous users can not create pages"));
                return RedirectToAction("index");
            }

            var page = _contentManager.Create<SandboxPage>("sandboxpage", item => {
                item.Record.Name = model.Name;
            });
            return RedirectToAction("show", new { page.ContentItem.Id });
        }


        public ActionResult Edit(int id) {
            var settings = CurrentSite.Get<ContentPart<SandboxSettingsRecord>>();
            if (settings.Record.AllowAnonymousEdits == false && CurrentUser == null) {
                _notifier.Error(T("Anonymous users can not edit pages"));
                return RedirectToAction("show", new { id });
            }

            var page = _contentManager.Get<SandboxPage>(id);
            var model = new PageEditViewModel {
                Page = _contentManager.BuildEditorModel(page)
            };
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(int id, FormCollection input) {
            var settings = CurrentSite.Get<ContentPart<SandboxSettingsRecord>>();
            if (settings.Record.AllowAnonymousEdits == false && CurrentUser == null) {
                _notifier.Error(T("Anonymous users can not edit pages"));
                return RedirectToAction("show", new { id });
            }

            var page = _contentManager.Get<SandboxPage>(id);
            var model = new PageEditViewModel {
                Page = _contentManager.UpdateEditorModel(page, this)
            };
            if (!ModelState.IsValid)
                return View(model);

            return RedirectToAction("show", new { id });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}

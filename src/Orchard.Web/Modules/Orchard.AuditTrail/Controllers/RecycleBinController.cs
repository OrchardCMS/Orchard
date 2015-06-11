using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.AuditTrail.ViewModels;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.AuditTrail.Controllers {
    [OrchardFeature("Orchard.AuditTrail.RecycleBin")]
    [Admin]
    public class RecycleBinController : Controller {
        private readonly IAuthorizer _authorizer;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IOrchardServices _services;
        private readonly IRecycleBin _recycleBin;

        public RecycleBinController(IAuthorizer authorizer, IContentManager contentManager, INotifier notifier, IOrchardServices services, IRecycleBin recycleBin) {
            _authorizer = authorizer;
            _contentManager = contentManager;
            _notifier = notifier;
            _services = services;
            _recycleBin = recycleBin;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index(PagerParameters pagerParameters, AuditTrailOrderBy? orderBy = null) {
            if (!_authorizer.Authorize(Permissions.ViewAuditTrail))
                return new HttpUnauthorizedResult();

            var viewModel = SetupViewModel(new RecycleBinViewModel(), pagerParameters);
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Restore(int id, string returnUrl) {
            var contentItem = _contentManager.Get(id, VersionOptions.AllVersions);
            if (!_authorizer.Authorize(Core.Contents.Permissions.PublishContent, contentItem))
                return new HttpUnauthorizedResult();

            var restoredContentItem = _recycleBin.Restore(contentItem);
            var restoredContentItemTitle = _contentManager.GetItemMetadata(restoredContentItem).DisplayText;

            _notifier.Information(T("&quot;{0}&quot; has been restored.", restoredContentItemTitle));

            return this.RedirectReturn(returnUrl, () => Url.Action("Index", "RecycleBin"));
        }

        [ActionName("Index")]
        [HttpPost]
        [FormValueRequired("ExecuteActionButton")]
        public ActionResult ExecuteAction(RecycleBinViewModel viewModel, PagerParameters pagerParameters) {
            if (viewModel.RecycleBinCommand == null) {
                ModelState.AddModelError("RecycleBinCommand", T("Please select an action to execute.").Text);
            }

            if (viewModel.SelectedContentItems == null || !viewModel.SelectedContentItems.Any()) {
                ModelState.AddModelError("SelectedContentItems", T("Please select one or more content items.").Text);
            }

            if (!ModelState.IsValid) {
                SetupViewModel(viewModel, pagerParameters);
                return View("Index", viewModel);
            }

            if (ModelState.IsValid) {
                var selectedContentItemIds = viewModel.SelectedContentItems.Where(x => x.Selected).Select(x => x.Id).ToArray();
                switch (viewModel.RecycleBinCommand) {
                    case RecycleBinCommand.Restore:
                        RestoreContentItems(selectedContentItemIds);
                        break;
                    case RecycleBinCommand.Destroy:
                        DeleteContentItems(selectedContentItemIds);
                        break;
                }
            }

            return RedirectToAction("Index");
        }

        private RecycleBinViewModel SetupViewModel(RecycleBinViewModel viewModel, PagerParameters pagerParameters) {
            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            var removedContentItems = _recycleBin.List(pager.Page, pager.PageSize);
            var pagershape = _services.New.Pager(pager).TotalItemCount(removedContentItems.TotalItemCount);

            viewModel.ContentItems = removedContentItems;
            viewModel.Pager = pagershape;

            return viewModel;
        }

        private void RestoreContentItems(IEnumerable<int> selectedContentItems) {
            var contentItems = _recycleBin.GetMany(selectedContentItems);

            foreach (var contentItem in contentItems) {
                var contentItemTitle = _contentManager.GetItemMetadata(contentItem).DisplayText;
                if (!_authorizer.Authorize(Core.Contents.Permissions.EditContent, contentItem)) {
                    _notifier.Error(T("You need the EditContent permission to restore <strong>{0}</strong>.", contentItemTitle));
                    continue;
                }

                _recycleBin.Restore(contentItem);
                _notifier.Information(T("&quot;{0}&quot; has been restored.", contentItemTitle));
            }
        }

        private void DeleteContentItems(IEnumerable<int> selectedContentItems) {
            var contentItems = _recycleBin.GetMany(selectedContentItems);

            foreach (var contentItem in contentItems) {
                var contentItemTitle = _contentManager.GetItemMetadata(contentItem).DisplayText;
                if (!_authorizer.Authorize(Core.Contents.Permissions.DeleteContent, contentItem)) {
                    _notifier.Error(T("You need the DeleteContent permission to permanently delete <strong>{0}</strong>.", contentItemTitle));
                    continue;
                }

                try {
                    _contentManager.Destroy(contentItem);
                    _notifier.Information(T("&quot;{0}&quot; has been permanently deleted.", contentItemTitle));
                }
                catch (Exception ex) {
                    Logger.Error(ex, "An exception occurred while trying to permanently delete content with ID {0}.", contentItem.Id);
                    _notifier.Error(T("An exception occurred while trying to permanently delete content with ID {0}.", contentItem.Id));
                }
                
            }
        }

    }
}
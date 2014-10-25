using System.Linq;
using System.Security.Policy;
using System.Web.Mvc;
using System.Web.UI;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.AuditTrail.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Settings;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.AuditTrail.Controllers {
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
        }

        public Localizer T { get; set; }

        public ActionResult Index(PagerParameters pagerParameters, AuditTrailOrderBy? orderBy = null) {
            if (!_authorizer.Authorize(Permissions.ViewAuditTrail))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            var removedContentItems = _recycleBin.List(pager.Page, pager.PageSize);
            var pagershape = _services.New.Pager(pager).TotalItemCount(removedContentItems.TotalItemCount);
            var viewModel = new RecycleBinViewModel {
                ContentItems = removedContentItems,
                Pager = pagershape
            };

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
    }
}
using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.ViewModels;
using Orchard.DisplayManagement.Shapes;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.AuditTrail.Controllers {
    public class AdminController : Controller {
        private readonly IAuthorizer _authorizer;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IOrchardServices _services;

        public AdminController(IAuditTrailManager auditTrailManager, IOrchardServices services) {
            _auditTrailManager = auditTrailManager;
            _services = services;
            _authorizer = services.Authorizer;
            New = _services.New;
        }

        public dynamic New { get; private set; }

        public ActionResult Index(PagerParameters pagerParameters) {
            if(!_authorizer.Authorize(Permissions.ManageAuditTrail))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            var pageOfData = _auditTrailManager.GetPage(pager.Page, pager.PageSize);
            var pagerShape = New.Pager(pager).TotalItemCount(pageOfData.TotalItemCount);
            var list = New.List();

            list.AddRange(pageOfData.Select(x => _auditTrailManager.BuildDisplay(x, "SummaryAdmin")).ToArray());
            var viewModel = new AuditTrailViewModel {
                Records = pageOfData,
                List = list,
                Pager = pagerShape
            };

            return View(viewModel);
        }

        public ActionResult Detail(int id) {
            if (!_authorizer.Authorize(Permissions.ManageAuditTrail))
                return new HttpUnauthorizedResult();

            var record = _auditTrailManager.GetRecord(id);
            var recordShape = _auditTrailManager.BuildDisplay(record, "Detail");
            var viewModel = new AuditTrailDetailsViewModel {
                Record = recordShape
            };
            return View(viewModel);
        }
    }
}
using System.Linq;
using System.Web.Mvc;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.AuditTrail.ViewModels;
using Orchard.Localization.Services;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.AuditTrail.Controllers {
    public class AdminController : Controller {
        private readonly IAuthorizer _authorizer;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IOrchardServices _services;
        private readonly IAuditTrailEventDisplayBuilder _displayBuilder;
        private readonly IDateServices _dateServices;

        public AdminController(IAuditTrailManager auditTrailManager, IOrchardServices services, IAuditTrailEventDisplayBuilder displayBuilder, IDateServices dateServices) {
            _auditTrailManager = auditTrailManager;
            _services = services;
            _displayBuilder = displayBuilder;
            _dateServices = dateServices;
            _authorizer = services.Authorizer;
            New = _services.New;
        }

        public dynamic New { get; private set; }

        public ActionResult Index(PagerParameters pagerParameters, AuditTrailOrderBy? orderBy = null) {

            if(!_authorizer.Authorize(Permissions.ViewAuditTrail))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            var filters = Filters.From(Request.QueryString);
            var pageOfData = _auditTrailManager.GetRecords(pager.Page, pager.PageSize, filters, orderBy ?? AuditTrailOrderBy.DateDescending);
            var pagerShape = New.Pager(pager).TotalItemCount(pageOfData.TotalItemCount);
            var filterLayout = _auditTrailManager.BuildFilterDisplays(filters);
            var eventDescriptorsQuery =
                from c in _auditTrailManager.DescribeCategories()
                from e in c.Events
                select e;
            var eventDescriptors = eventDescriptorsQuery.ToDictionary(x => x.Event);
            var recordViewModelsQuery =
                from record in pageOfData
                let descriptor = eventDescriptors.ContainsKey(record.Event) ? eventDescriptors[record.Event] : default(AuditTrailEventDescriptor)
                where descriptor != null
                select new AuditTrailEventSummaryViewModel {
                    Record = record,
                    EventDescriptor = descriptor,
                    CategoryDescriptor = descriptor.CategoryDescriptor,
                    SummaryShape = _displayBuilder.BuildDisplay(record, "SummaryAdmin")
                };

            var viewModel = new AuditTrailViewModel {
                Records = recordViewModelsQuery.ToArray(),
                Pager = pagerShape,
                OrderBy = orderBy ?? AuditTrailOrderBy.DateDescending,
                FilterLayout = filterLayout
            };

            return View(viewModel);
        }

        public ActionResult Detail(int id) {
            if (!_authorizer.Authorize(Permissions.ViewAuditTrail))
                return new HttpUnauthorizedResult();

            var record = _auditTrailManager.GetRecord(id);
            var descriptor = _auditTrailManager.DescribeEvent(record.Event);
            var detailsShape = _displayBuilder.BuildDisplay(record, "Detail");
            var viewModel = new AuditTrailDetailsViewModel {
                Record = record,
                Descriptor = descriptor,
                DetailsShape = detailsShape
            };
            return View(viewModel);
        }
    }
}
using System.Linq;
using System.Web.Mvc;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.AuditTrail.ViewModels;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.AuditTrail.Controllers {
    public class AdminController : Controller, IUpdateModel {
        private readonly IAuthorizer _authorizer;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IOrchardServices _services;
        private readonly IAuditTrailEventDisplayBuilder _displayBuilder;

        public AdminController(IAuditTrailManager auditTrailManager, IOrchardServices services, IAuditTrailEventDisplayBuilder displayBuilder) {
            _auditTrailManager = auditTrailManager;
            _services = services;
            _displayBuilder = displayBuilder;
            _authorizer = services.Authorizer;
            New = _services.New;
        }

        public dynamic New { get; private set; }

        public ActionResult Index(PagerParameters pagerParameters, AuditTrailOrderBy? orderBy = null) {
            if(!_authorizer.Authorize(Permissions.ViewAuditTrail))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            var filters = Filters.From(Request.QueryString, this);
            var pageOfData = _auditTrailManager.GetRecords(pager.Page, pager.PageSize, filters, orderBy ?? AuditTrailOrderBy.DateDescending);

            // If there's a filter validation error, clear the results.
            if (!ModelState.IsValid) {
                pageOfData = new PageOfItems<AuditTrailEventRecord>(Enumerable.Empty<AuditTrailEventRecord>());
            }

            var pagerShape = New.Pager(pager).TotalItemCount(pageOfData.TotalItemCount);
            var filterDisplay = _auditTrailManager.BuildFilterDisplay(filters);
            var eventDescriptorsQuery =
                from c in _auditTrailManager.DescribeCategories()
                from e in c.Events
                select e;
            var eventDescriptors = eventDescriptorsQuery.ToDictionary(x => x.Event);
            var recordViewModelsQuery =
                from record in pageOfData
                let descriptor = eventDescriptors.ContainsKey(record.FullEventName) ? eventDescriptors[record.FullEventName] : AuditTrailEventDescriptor.Basic(record)
                select new AuditTrailEventSummaryViewModel {
                    Record = record,
                    EventDescriptor = descriptor,
                    CategoryDescriptor = descriptor.CategoryDescriptor,
                    SummaryShape = _displayBuilder.BuildDisplay(record, "SummaryAdmin"),
                    ActionsShape = _displayBuilder.BuildActions(record, "SummaryAdmin"),
                };

            var viewModel = new AuditTrailViewModel {
                Records = recordViewModelsQuery.ToArray(),
                Pager = pagerShape,
                OrderBy = orderBy ?? AuditTrailOrderBy.DateDescending,
                FilterDisplay = filterDisplay
            };

            return View(viewModel);
        }

        public ActionResult Detail(int id) {
            if (!_authorizer.Authorize(Permissions.ViewAuditTrail))
                return new HttpUnauthorizedResult();

            var record = _auditTrailManager.GetRecord(id);
            var descriptor = _auditTrailManager.DescribeEvent(record);
            var detailsShape = _displayBuilder.BuildDisplay(record, "Detail");
            var viewModel = new AuditTrailDetailsViewModel {
                Record = record,
                Descriptor = descriptor,
                DetailsShape = detailsShape
            };
            return View(viewModel);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}
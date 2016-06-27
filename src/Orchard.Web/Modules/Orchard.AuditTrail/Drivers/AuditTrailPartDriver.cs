using System.Collections.Generic;
using System.Linq;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Providers.Content;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Settings;
using Orchard.AuditTrail.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.UI.Navigation;

namespace Orchard.AuditTrail.Drivers {
    public class AuditTrailPartDriver : ContentPartDriver<AuditTrailPart> {
        private readonly IOrchardServices _services;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IAuditTrailEventDisplayBuilder _displayBuilder;

        public AuditTrailPartDriver(IOrchardServices services, IAuditTrailManager auditTrailManager, IAuditTrailEventDisplayBuilder displayBuilder) {
            _services = services;
            _auditTrailManager = auditTrailManager;
            _displayBuilder = displayBuilder;
        }

        protected override DriverResult Editor(AuditTrailPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AuditTrailPart part, IUpdateModel updater, dynamic shapeHelper) {
            var settings = part.Settings.GetModel<AuditTrailPartSettings>();
            var results = new List<DriverResult>();

            if (settings.ShowAuditTrailCommentInput) {
                results.Add(ContentShape("Parts_AuditTrail_Comment", () => {
                    var viewModel = new AuditTrailCommentViewModel();

                    if (part.ShowComment) {
                        viewModel.Comment = part.Comment;
                    }

                    if (updater != null) {
                        if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                            part.Comment = viewModel.Comment;
                        }
                    }
                    return shapeHelper.EditorTemplate(Model: viewModel, TemplateName: "Parts.AuditTrail.Comment", Prefix: Prefix);
                }));
            }
            if (_services.Authorizer.Authorize(Permissions.ViewAuditTrail)) {
                if (settings.ShowAuditTrailLink) {
                    results.Add(ContentShape("Parts_AuditTrail_Link", () => shapeHelper.Parts_AuditTrail_Link()));
                }

                if (settings.ShowAuditTrail) {
                    results.Add(ContentShape("Parts_AuditTrail", () => {
                        var pager = new Pager(_services.WorkContext.CurrentSite, null, null);
                        var pageOfData = _auditTrailManager.GetRecords(pager.Page, pager.PageSize, ContentAuditTrailEventProvider.CreateFilters(part.Id, updater));
                        var pagerShape = shapeHelper.Pager(pager).TotalItemCount(pageOfData.TotalItemCount);
                        var eventDescriptorsQuery =
                            from c in _auditTrailManager.DescribeCategories()
                            from e in c.Events
                            select e;
                        var eventDescriptors = eventDescriptorsQuery.ToArray();
                        var recordViewModelsQuery =
                            from record in pageOfData
                            let descriptor = eventDescriptors.FirstOrDefault(x => x.Event == record.FullEventName)
                            where descriptor != null
                            select new AuditTrailEventSummaryViewModel {
                                Record = record,
                                EventDescriptor = descriptor,
                                CategoryDescriptor = descriptor.CategoryDescriptor,
                                SummaryShape = _displayBuilder.BuildDisplay(record, "SummaryAdmin"),
                                ActionsShape = _displayBuilder.BuildActions(record, "SummaryAdmin")
                            };
                        var recordViewModels = recordViewModelsQuery.ToArray();
                        return shapeHelper.Parts_AuditTrail(Records: recordViewModels, Pager: pagerShape);
                    }));
                }
            }

            return Combined(results.ToArray());
        }
    }
}
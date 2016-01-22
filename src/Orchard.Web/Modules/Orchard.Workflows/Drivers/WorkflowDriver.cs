using System.Linq;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Drivers {
    public class WorkflowDriver : ContentPartDriver<CommonPart> {
        private readonly IRepository<WorkflowRecord> _workflowRepository;

        public WorkflowDriver(
            IOrchardServices services,
            IRepository<WorkflowRecord> workflowRepository
            ) {
                _workflowRepository = workflowRepository;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "WorkflowDriver"; }
        }

        protected override DriverResult Display(CommonPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Workflow_SummaryAdmin", () => {
                var workflows = _workflowRepository.Table.Where(x => x.ContentItemRecord == part.ContentItem.Record).ToList();
                return shapeHelper.Parts_Workflow_SummaryAdmin().Workflows(workflows);
            });
        }
    }
}
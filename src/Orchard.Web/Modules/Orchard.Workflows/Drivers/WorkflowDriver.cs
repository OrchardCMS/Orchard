using System.Linq;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Drivers {
    public class WorkflowDriver : ContentPartDriver<CommonPart> {
        private readonly IRepository<AwaitingActivityRecord> _awaitingActivityRepository;

        public WorkflowDriver(
            IOrchardServices services,
            IRepository<AwaitingActivityRecord> awaitingActivityRepository
            ) {
            _awaitingActivityRepository = awaitingActivityRepository;
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
                var awaiting = _awaitingActivityRepository.Table.Where(x => x.ContentItemRecord == part.ContentItem.Record).ToList();
                return shapeHelper.Parts_Workflow_SummaryAdmin().Activities(awaiting.Select(x => x.ActivityRecord));
            });
        }
    }
}
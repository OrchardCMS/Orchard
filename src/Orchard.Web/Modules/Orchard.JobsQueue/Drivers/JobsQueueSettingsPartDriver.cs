using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.JobsQueue.Models;
using Orchard.JobsQueue.ViewModels;
using Orchard.Messaging.Models;

namespace Orchard.JobsQueue.Drivers {
    public class JobsQueueSettingsPartDriver : ContentPartDriver<JobsQueueSettingsPart> {
        private const string TemplateName = "Parts/JobsQueueSettings";
        public IOrchardServices Services { get; set; }

        public JobsQueueSettingsPartDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "JobsQueueSettings"; } }

        protected override DriverResult Editor(JobsQueueSettingsPart part, dynamic shapeHelper) {

            var model = new JobsQueueSettingsPartViewModel {
                JobsQueueSettings = part
            };

            return ContentShape("Parts_JobsQueueSettings_Edit", () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(JobsQueueSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new JobsQueueSettingsPartViewModel {
                JobsQueueSettings = part
            };

            updater.TryUpdateModel(model, Prefix, null, null);

            return ContentShape("Parts_JobsQueueSettings_Edit", () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }
    }
}
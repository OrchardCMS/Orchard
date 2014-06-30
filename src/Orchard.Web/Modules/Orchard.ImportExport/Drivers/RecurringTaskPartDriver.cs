using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;

namespace Orchard.ImportExport.Drivers {
    [OrchardFeature("Orchard.Deployment")]
    public class RecurringTaskPartDriver : ContentPartDriver<RecurringTaskPart> {
        private readonly IRecurringScheduledTaskManager _taskManager;

        public RecurringTaskPartDriver(IRecurringScheduledTaskManager taskManager,
            IOrchardServices services) {
            _taskManager = taskManager;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        //GET
        protected override DriverResult Editor(RecurringTaskPart part, dynamic shapeHelper) {
            var model = new RecurringTaskViewModel {
                IsActive = part.IsActive,
                RepeatFrequencyInMinutes = part.RepeatFrequencyInMinutes,
            };

            return ContentShape("Parts_RecurringTask_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Deployment.RecurringTask",
                    Model: model,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            RecurringTaskPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new RecurringTaskViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);
            part.RepeatFrequencyInMinutes = model.RepeatFrequencyInMinutes;
            part.IsActive = model.IsActive;

            if (model.IsActive)//Schedule date time is not required if the task is run on demand
            {
                _taskManager.ScheduleTaskForNextRun(part, true);
            }

            return Editor(part, shapeHelper);
        }
    }
}

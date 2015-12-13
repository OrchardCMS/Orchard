using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Services;
using Orchard.Tasks;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
    [OrchardFeature("Orchard.Workflows.Timer")]
    public class TimerActivity : Event {
        private readonly IClock _clock;
        private readonly IDateLocalizationServices _dateServices;

        public TimerActivity(IClock clock, IDateLocalizationServices dateServices) {
            _clock = clock;
            _dateServices = dateServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "Timer"; }
        }

        public override LocalizedString Category {
            get { return T("Tasks"); }
        }

        public override LocalizedString Description {
            get { return T("Wait for a specific time has passed."); }
        }

        public override string Form {
            get { return "ActivityTimer"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return IsExpired(workflowContext, activityContext);
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            if(IsExpired(workflowContext, activityContext)) {
                yield return T("Done");
            }
        }

        private bool IsExpired(WorkflowContext workflowContext, ActivityContext activityContext) {
            DateTime started;

            if (!workflowContext.HasStateFor(activityContext.Record, "StartedUtc")) {
                var dateString = activityContext.GetState<string>("Date");
                var date = _dateServices.ConvertFromLocalizedString(dateString);
                started = date ?? _clock.UtcNow;

                workflowContext.SetStateFor(activityContext.Record, "StartedUtc", started);
            }
            else {
                started = workflowContext.GetStateFor<DateTime>(activityContext.Record, "StartedUtc");
            }

            var amount = activityContext.GetState<int>("Amount");
            var type = activityContext.GetState<string>("Unity");

            return _clock.UtcNow > When(started, amount, type);
        }

        public static DateTime When(DateTime started, int amount, string type) {
            try {
                var when = started;

                switch (type) {
                    case "Minute":
                        when = when.AddMinutes(amount);
                        break;
                    case "Hour":
                        when = when.AddHours(amount);
                        break;
                    case "Day":
                        when = when.AddDays(amount);
                        break;
                    case "Week":
                        when = when.AddDays(7*amount);
                        break;
                    case "Month":
                        when = when.AddMonths(amount);
                        break;
                    case "Year":
                        when = when.AddYears(amount);
                        break;
                }

                return when;
            }
            catch {
                return DateTime.MaxValue;
            }
        }
    }

    [OrchardFeature("Orchard.Workflows.Timer")]
    public class TimerBackgroundTask : IBackgroundTask {
        private readonly IContentManager _contentManager;
        private readonly IWorkflowManager _workflowManager;
        private readonly IRepository<AwaitingActivityRecord> _awaitingActivityRepository;

        public TimerBackgroundTask(
            IContentManager contentManager,
            IWorkflowManager workflowManager,
            IRepository<AwaitingActivityRecord> awaitingActivityRepository) {
            _contentManager = contentManager;
            _workflowManager = workflowManager;
            _awaitingActivityRepository = awaitingActivityRepository;
        }

        public void Sweep() {
            var awaiting = _awaitingActivityRepository.Table.Where(x => x.ActivityRecord.Name == "Timer").ToList();
            
            
            foreach (var action in awaiting) {
                var contentItem = _contentManager.Get(action.WorkflowRecord.ContentItemRecord.Id, VersionOptions.Latest);
                var tokens = new Dictionary<string, object> { { "Content", contentItem } };
                var workflowState = FormParametersHelper.FromJsonString(action.WorkflowRecord.State);
                workflowState.TimerActivity_StartedUtc = null;
                action.WorkflowRecord.State = FormParametersHelper.ToJsonString(workflowState);
                _workflowManager.TriggerEvent("Timer", contentItem, () => tokens);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Services;
using Orchard.Tasks;
using Orchard.Workflows.Models;
using Orchard.Workflows.Models.Descriptors;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
    public class TimerActivity : Event {
        private readonly IClock _clock;
        private readonly IWorkContextAccessor _workContextAccessor;

        public TimerActivity(
            IClock clock,
            IWorkContextAccessor workContextAccessor) {
            _clock = clock;
            _workContextAccessor = workContextAccessor;
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

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(ActivityContext context) {
            yield return T("Done");
        }

        public override bool CanExecute(ActivityContext context) {

            return _clock.UtcNow > When(context);
        }

        public override void Touch(dynamic workflowState) {
            workflowState.TimerActivity_StartedUtc = _clock.UtcNow;
        }

        public override IEnumerable<LocalizedString> Execute(ActivityContext context) {
            if(_clock.UtcNow > When(context)) {
                yield return T("Done");
            }
        }

        public static DateTime When(ActivityContext context) {
            try {
                int amount = context.State.Amount;
                string type = context.State.Unity;
                
                DateTime started = context.WorkflowState.TimerActivity_StartedUtc;

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

    public class TimerBackgroundTask : IBackgroundTask {
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IWorkflowManager _workflowManager;
        private readonly IRepository<AwaitingActivityRecord> _awaitingActivityRepository;

        public TimerBackgroundTask(
            IClock clock,
            IContentManager contentManager,
            IWorkflowManager workflowManager,
            IRepository<AwaitingActivityRecord> awaitingActivityRepository) {
            _clock = clock;
            _contentManager = contentManager;
            _workflowManager = workflowManager;
            _awaitingActivityRepository = awaitingActivityRepository;
        }

        public void Sweep() {
            var awaiting = _awaitingActivityRepository.Table.Where(x => x.ActivityRecord.Name == "Timer").ToList();
            var actions = awaiting.Where(x => {
                var contentItem = _contentManager.Get(x.ContentItemRecord.Id, VersionOptions.Latest);
                var state = FormParametersHelper.FromJsonString(x.ActivityRecord.State);
                var workflowState = FormParametersHelper.FromJsonString(x.WorkflowRecord.State);
                return _clock.UtcNow > TimerActivity.When(new ActivityContext {
                    State = state, 
                    WorkflowState = workflowState, 
                    Content = contentItem
                });
            });

            foreach (var action in actions) {
                var contentItem = _contentManager.Get(action.ContentItemRecord.Id, VersionOptions.Latest);
                _workflowManager.TriggerEvent("Timer", contentItem, () => new Dictionary<string, object> { { "Content", contentItem } });
            }
        }
    }
}
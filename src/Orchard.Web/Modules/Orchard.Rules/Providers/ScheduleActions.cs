using System;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Rules.Models;
using Orchard.Rules.Services;
using Orchard.Localization;
using Orchard.Services;
using Orchard.Mvc.Html;
using Orchard.Tasks.Scheduling;
using Orchard.Tokens;

namespace Orchard.Rules.Providers {
    public class ScheduleActions : IActionProvider {
        private readonly IContentManager _contentManager;
        private readonly IScheduledTaskManager _scheduledTaskManager;
        private readonly IRepository<RuleRecord> _repository;
        private readonly IRepository<ActionRecord> _actionRecordRepository;
        private readonly IRepository<ScheduledActionRecord> _scheduledActionRecordRepository;
        private readonly IClock _clock;
        private readonly ITokenizer _tokenizer;

        public ScheduleActions(
            IContentManager contentManager,
            IScheduledTaskManager scheduledTaskManager,
            IRepository<RuleRecord> repository,
            IRepository<ActionRecord> actionRecordRepository,
            IRepository<ScheduledActionRecord> scheduledActionRecordRepository,
            IClock clock,
            ITokenizer tokenizer) {
            _contentManager = contentManager;
            _scheduledTaskManager = scheduledTaskManager;
            _repository = repository;
            _actionRecordRepository = actionRecordRepository;
            _scheduledActionRecordRepository = scheduledActionRecordRepository;
            _clock = clock;
            _tokenizer = tokenizer;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeActionContext context) {
            context.For("System", T("System"), T("System"))
                .Element("Delayed", T("Delayed Action"), T("Triggers some actions after a specific amount of time."), CreateDelayedAction, DisplayDelayedAction, "ActionDelay");
        }

        private LocalizedString DisplayDelayedAction(ActionContext context) {
            var amount = Convert.ToInt32(context.Properties["Amount"]);
            var type = context.Properties["Unity"];
            var ruleId = Convert.ToInt32(context.Properties["RuleId"]);

            var rule = _repository.Get(ruleId);

            return T.Plural("Triggers \"{1}\" in {0} {2}", "Triggers \"{1}\" in {0} {2}s", amount, rule.Name, type);
        }

        private bool CreateDelayedAction(ActionContext context) {
            var amount = Convert.ToInt32(context.Properties["Amount"]);
            var type = context.Properties["Unity"];
            var ruleId = Convert.ToInt32(context.Properties["RuleId"]);

            var scheduledActionTask = _contentManager.New("ScheduledActionTask").As<ScheduledActionTaskPart>();
            var rule = _repository.Get(ruleId);

            var when = _clock.UtcNow;

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
                    when = when.AddDays(7 * amount);
                    break;
                case "Month":
                    when = when.AddMonths(amount);
                    break;
                case "Year":
                    when = when.AddYears(amount);
                    break;
            }

            foreach (var action in rule.Actions) {
                var actionRecord = new ActionRecord {
                    Category = action.Category,
                    Position = action.Position,
                    Type = action.Type,
                    Parameters = _tokenizer.Replace(action.Parameters, context.Tokens)
                };

                _actionRecordRepository.Create(actionRecord);

                var scheduledAction = new ScheduledActionRecord { ActionRecord = actionRecord };
                _scheduledActionRecordRepository.Create(scheduledAction);

                scheduledActionTask.ScheduledActions.Add(scheduledAction);
            }


            _contentManager.Create(scheduledActionTask, VersionOptions.Draft);

            _scheduledTaskManager.CreateTask("TriggerRule", when, scheduledActionTask.ContentItem);

            return true;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Logging;
using Orchard.Rules.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Tokens;

namespace Orchard.Rules.Services {
    public class RulesManager : IRulesManager {
        private readonly IRepository<EventRecord> _eventRepository;
        private readonly IRepository<RuleRecord> _ruleRepository;
        private readonly IEnumerable<IEventProvider> _eventProviders;
        private readonly IEnumerable<IActionProvider> _actionProviders;
        private readonly ITokenizer _tokenizer;

        public RulesManager(
            IRepository<EventRecord> eventRepository,
            IRepository<RuleRecord> ruleRepository,
            IEnumerable<IEventProvider> eventProviders,
            IEnumerable<IActionProvider> actionProviders,
            ITokenizer tokenizer) {
            _eventRepository = eventRepository;
            _ruleRepository = ruleRepository;
            _eventProviders = eventProviders;
            _actionProviders = actionProviders;
            _tokenizer = tokenizer;

            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public IEnumerable<TypeDescriptor<EventDescriptor>> DescribeEvents() {
            var context = new DescribeEventContext();
            foreach (var provider in _eventProviders) {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public IEnumerable<TypeDescriptor<ActionDescriptor>> DescribeActions() {
            var context = new DescribeActionContext();
            foreach (var provider in _actionProviders) {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public void TriggerEvent(string category, string type, Func<Dictionary<string, object>> tokensContext) {
            var tokens = tokensContext();
            var eventDescriptors = DescribeEvents().SelectMany(x => x.Descriptors).ToList();

            // load corresponding events, as on one Rule several events of the same type could be configured 
            // with different parameters
            var events = _eventRepository.Table
                .Where(x => x.Category == category && x.Type == type && x.RuleRecord.Enabled)
                .ToList() // execute the query at this point of time
                .Where(e => { // take the first event which has a valid condition
                    var eventCategory = e.Category;
                    var eventType = e.Type;

                    // look for the specified Event target/type
                    var descriptor = eventDescriptors.FirstOrDefault(x => eventCategory == x.Category && eventType == x.Type);

                    if (descriptor == null) {
                        return false;
                    }

                    var properties = FormParametersHelper.FromString(e.Parameters);
                    var context = new EventContext { Tokens = tokens, Properties = properties };

                    // check the condition
                    return descriptor.Condition(context);

                })
                .ToList();

            // if no events are true simply do nothing and return
            if(!events.Any()) {
                 return;
            }

            // load rules too for eager loading
            var rules = _ruleRepository.Table
                .Where(x => x.Enabled && x.Events.Any(e => e.Category == category && e.Type == type));

            // evaluate their conditions
            foreach (var e in events) {
                var rule = e.RuleRecord;

                ExecuteActions(rule.Actions.OrderBy(x => x.Position), tokens);
            }
        }

        public void ExecuteActions(IEnumerable<ActionRecord> actions, Dictionary<string, object> tokens) {
            var actionDescriptors = DescribeActions().SelectMany(x => x.Descriptors).ToList();

            // execute each action associated with this rule
            foreach (var actionRecord in actions) {
                var actionCategory = actionRecord.Category;
                var actionType = actionRecord.Type;

                // look for the specified Event target/type
                var descriptor = actionDescriptors.FirstOrDefault(x => actionCategory == x.Category && actionType == x.Type);

                if (descriptor == null) {
                    continue;
                }

                // evaluate the tokens
                var parameters = _tokenizer.Replace(actionRecord.Parameters, tokens);

                var properties = FormParametersHelper.FromString(parameters);
                var context = new ActionContext { Properties = properties, Tokens = tokens };

                // execute the action
                try {
                    var continuation = descriptor.Action(context);

                    // early termination of the actions ?
                    if (!continuation) {
                        break;
                    }
                }
                catch (Exception e) {
                    Logger.Error(e, "An action could not be executed.");
                    
                    // stop executing other actions
                    break;
                }
            }
        }
    }
}
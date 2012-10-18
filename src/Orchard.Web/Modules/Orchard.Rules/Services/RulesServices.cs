using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Rules.Models;
using Orchard.Data;
using Orchard.Localization;

namespace Orchard.Rules.Services {
    public class RulesServices : IRulesServices {
        private readonly IRepository<EventRecord> _eventRepository;
        private readonly IRepository<ActionRecord> _actionRepository;
        private readonly IRepository<RuleRecord> _ruleRepository;

        public RulesServices(
            IRepository<EventRecord> eventRepository,
            IRepository<ActionRecord> actionRepository,
            IRepository<RuleRecord> ruleRepository) {
            _eventRepository = eventRepository;
            _actionRepository = actionRepository;
            _ruleRepository = ruleRepository;
        }

        public Localizer T { get; set; }

        public RuleRecord CreateRule(string name) {
            var ruleRecord = new RuleRecord { Name = name };
            _ruleRepository.Create(ruleRecord);

            return ruleRecord;
        }

        public RuleRecord GetRule(int id) {
            return _ruleRepository.Get(id);
        }

        public IEnumerable<RuleRecord> GetRules() {
            return _ruleRepository.Table.ToList();
        }

        public void DeleteRule(int ruleId) {
            var e = _ruleRepository.Get(ruleId);
            if (e != null) {
                _ruleRepository.Delete(e);
            }
        }

        public void DeleteEvent(int eventId) {
            var e = _eventRepository.Get(eventId);
            if (e != null) {
                _eventRepository.Delete(e);
            }
        }

        public void DeleteAction(int actionId) {
            var a = _actionRepository.Get(actionId);
            if (a != null) {
                _actionRepository.Delete(a);
            }
        }

        public void MoveUp(int actionId) {
            var action = _actionRepository.Get(actionId);

            // look for the previous action in order in same rule
            var previous = _actionRepository.Table
                .Where(x => x.Position < action.Position && x.RuleRecord.Id == action.RuleRecord.Id)
                .OrderByDescending(x => x.Position)
                .FirstOrDefault();

            // nothing to do if already at the top
            if (previous == null) {
                return;
            }

            // switch positions
            var temp = previous.Position;
            previous.Position = action.Position;
            action.Position = temp;
        }

        public void MoveDown(int actionId) {
            var action = _actionRepository.Get(actionId);

            // look for the next action in order in same rule
            var next = _actionRepository.Table
                .Where(x => x.Position > action.Position && x.RuleRecord.Id == action.RuleRecord.Id)
                .OrderBy(x => x.Position)
                .FirstOrDefault();

            // nothing to do if already at the end
            if (next == null) {
                return;
            }

            // switch positions
            var temp = next.Position;
            next.Position = action.Position;
            action.Position = temp;
        }
    }
}
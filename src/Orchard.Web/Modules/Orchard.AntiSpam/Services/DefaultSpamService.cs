using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Rules;
using Orchard.AntiSpam.Settings;
using Orchard.ContentManagement;
using Orchard.Tokens;

namespace Orchard.AntiSpam.Services {
    public class DefaultSpamService : ISpamService {
        private readonly ITokenizer _tokenizer;
        private readonly IEnumerable<ISpamFilterProvider> _providers;
        private readonly ISpamEventHandler _spamEventHandler;
        private readonly IRulesManager _rulesManager;

        public DefaultSpamService(
            ITokenizer tokenizer, 
            IEnumerable<ISpamFilterProvider> providers,
            ISpamEventHandler spamEventHandler,
            IRulesManager rulesManager
            ) {
            _tokenizer = tokenizer;
            _providers = providers;
            _spamEventHandler = spamEventHandler;
            _rulesManager = rulesManager;
        }

        public SpamStatus CheckForSpam(string text, SpamFilterAction action, IContent content) {

            if (string.IsNullOrWhiteSpace(text)) {
                return SpamStatus.Ham;
            }

            var spamFilters = GetSpamFilters().ToList();

            var result = SpamStatus.Ham;

            switch (action) {
                case SpamFilterAction.AllOrNothing:
                    if (spamFilters.All(x => x.CheckForSpam(text) == SpamStatus.Spam)) {
                        result = SpamStatus.Spam;
                    }

                    break;
                case SpamFilterAction.One:
                    if (spamFilters.Any(x => x.CheckForSpam(text) == SpamStatus.Spam)) {
                        result =  SpamStatus.Spam;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // trigger events and rules
            switch (result) {
                case SpamStatus.Spam:
                    _spamEventHandler.SpamReported(content);
                    _rulesManager.TriggerEvent("AntiSpam", "Spam", () => new Dictionary<string, object> { { "Content", content } });
                    break;
                case SpamStatus.Ham:
                    _spamEventHandler.HamReported(content);
                    _rulesManager.TriggerEvent("AntiSpam", "Ham", () => new Dictionary<string, object> { { "Content", content } });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        public SpamStatus CheckForSpam(SpamFilterPart part) {

            var settings = part.TypePartDefinition.Settings.GetModel<SpamFilterPartSettings>();

            // evaluate the text to submit to the spam filters
            var text = _tokenizer.Replace(settings.Pattern, new Dictionary<string, object> { { "Content", part.ContentItem } });

            if (string.IsNullOrWhiteSpace(text)) {
                return SpamStatus.Ham;
            }

            var result = CheckForSpam(text, settings.Action, part);

            return result;
        }

        public void ReportSpam(string text) {
            var spamFilters = GetSpamFilters().ToList();

            foreach(var filter in spamFilters) {
                filter.ReportSpam(text);
            }
        }

        public void ReportSpam(SpamFilterPart part) {
            var settings = part.TypePartDefinition.Settings.GetModel<SpamFilterPartSettings>();

            // evaluate the text to submit to the spam filters
            var text = _tokenizer.Replace(settings.Pattern, new Dictionary<string, object> { { "Content", part.ContentItem } });

            ReportSpam(text);
        }

        public void ReportHam(string text) {
            var spamFilters = GetSpamFilters().ToList();

            foreach (var filter in spamFilters) {
                filter.ReportHam(text);
            }
        }

        public void ReportHam(SpamFilterPart part) {
            var settings = part.TypePartDefinition.Settings.GetModel<SpamFilterPartSettings>();

            // evaluate the text to submit to the spam filters
            var text = _tokenizer.Replace(settings.Pattern, new Dictionary<string, object> { { "Content", part.ContentItem } });

            ReportHam(text);
        }

        public IEnumerable<ISpamFilter> GetSpamFilters() {
            return _providers.SelectMany(x => x.GetSpamFilters()).Where(x => x != null);
        }
    }
}
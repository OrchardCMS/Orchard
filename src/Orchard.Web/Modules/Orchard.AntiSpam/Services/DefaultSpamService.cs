using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Rules;
using Orchard.AntiSpam.Settings;
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

        public SpamStatus CheckForSpam(string text, SpamFilterAction action) {
            var spamFilters = GetSpamFilters().ToList();

            switch (action) {
                case SpamFilterAction.AllOrNothing:
                    if (spamFilters.All(x => x.CheckForSpam(text) == SpamStatus.Spam)) {
                        return SpamStatus.Spam;
                    }

                    return SpamStatus.Ham;
                case SpamFilterAction.One:
                    if (spamFilters.Any(x => x.CheckForSpam(text) == SpamStatus.Spam)) {
                        return SpamStatus.Spam;
                    }

                    return SpamStatus.Ham;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public SpamStatus CheckForSpam(SpamFilterPart part) {

            var settings = part.TypePartDefinition.Settings.GetModel<SpamFilterPartSettings>();

            // evaluate the text to submit to the spam filters
            var text = _tokenizer.Replace(settings.Pattern, new Dictionary<string, object> { { "Content", part.ContentItem } });

            var result = CheckForSpam(text, settings.Action);

            // trigger events and rules
            switch (result) {
                case SpamStatus.Spam:
                    _spamEventHandler.SpamReported(part);
                    _rulesManager.TriggerEvent("AntiSpam", "Spam", () => new Dictionary<string, object> { { "Content", part.ContentItem } });
                    break;
                case SpamStatus.Ham:
                    _spamEventHandler.HamReported(part);
                    _rulesManager.TriggerEvent("AntiSpam", "Ham", () => new Dictionary<string, object> { { "Content", part.ContentItem } });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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
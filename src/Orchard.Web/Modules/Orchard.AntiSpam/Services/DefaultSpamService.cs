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
        private readonly IWorkContextAccessor _workContextAccessor;

        public DefaultSpamService(
            ITokenizer tokenizer, 
            IEnumerable<ISpamFilterProvider> providers,
            ISpamEventHandler spamEventHandler,
            IRulesManager rulesManager,
            IWorkContextAccessor workContextAccessor
            ) {
            _tokenizer = tokenizer;
            _providers = providers;
            _spamEventHandler = spamEventHandler;
            _rulesManager = rulesManager;
            _workContextAccessor = workContextAccessor;
        }

        public SpamStatus CheckForSpam(CommentCheckContext context, SpamFilterAction action, IContent content) {

            if (string.IsNullOrWhiteSpace(context.CommentContent)) {
                return SpamStatus.Ham;
            }

            var spamFilters = GetSpamFilters().ToList();

            var result = SpamStatus.Ham;

            switch (action) {
                case SpamFilterAction.AllOrNothing:
                    if (spamFilters.All(x => x.CheckForSpam(context) == SpamStatus.Spam)) {
                        result = SpamStatus.Spam;
                    }

                    break;
                case SpamFilterAction.One:
                    if (spamFilters.Any(x => x.CheckForSpam(context) == SpamStatus.Spam)) {
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
            var context = CreateCommentCheckContext(part, _workContextAccessor.GetContext());

            if (string.IsNullOrWhiteSpace(context.CommentContent)) {
                return SpamStatus.Ham;
            }

            var result = CheckForSpam(context, settings.Action, part);

            return result;
        }

        public void ReportSpam(CommentCheckContext context) {
            var spamFilters = GetSpamFilters().ToList();

            foreach(var filter in spamFilters) {
                filter.ReportSpam(context);
            }
        }

        public void ReportSpam(SpamFilterPart part) {
           ReportSpam(CreateCommentCheckContext(part, _workContextAccessor.GetContext()));
        }

        public void ReportHam(CommentCheckContext context) {
            var spamFilters = GetSpamFilters().ToList();

            foreach (var filter in spamFilters) {
                filter.ReportHam(context);
            }
        }

        public void ReportHam(SpamFilterPart part) {
            ReportHam(CreateCommentCheckContext(part, _workContextAccessor.GetContext()));
        }

        public IEnumerable<ISpamFilter> GetSpamFilters() {
            return _providers.SelectMany(x => x.GetSpamFilters()).Where(x => x != null);
        }

        private CommentCheckContext CreateCommentCheckContext(SpamFilterPart part, WorkContext workContext) {
            var settings = part.TypePartDefinition.Settings.GetModel<SpamFilterPartSettings>();

            var data = new Dictionary<string, object> {{"Content", part.ContentItem}};

            var context = new CommentCheckContext {
                Url = _tokenizer.Replace(settings.UrlPattern, data),
                Permalink = _tokenizer.Replace(settings.PermalinkPattern, data),
                CommentAuthor = _tokenizer.Replace(settings.CommentAuthorPattern, data),
                CommentAuthorEmail = _tokenizer.Replace(settings.CommentAuthorEmailPattern, data),
                CommentAuthorUrl = _tokenizer.Replace(settings.CommentAuthorUrlPattern, data),
                CommentContent = _tokenizer.Replace(settings.CommentContentPattern, data),
                CommentType = part.ContentItem.ContentType.ToLower()
            };

            if(workContext.HttpContext != null) {
                context.UserIp = workContext.HttpContext.Request.ServerVariables["REMOTE_ADDR"];
                context.UserAgent = workContext.HttpContext.Request.UserAgent;
                context.Referrer = Convert.ToString(workContext.HttpContext.Request.UrlReferrer);
            }

            return context;
        }
    }
}
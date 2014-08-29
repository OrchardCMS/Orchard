using System;
using System.Globalization;
using System.Linq;
using Orchard.Events;
using Orchard.Localization.Services;
using Orchard.Mvc;

namespace Orchard.Localization.RuleEngine {
    public interface IRuleProvider : IEventHandler {
        void Process(dynamic ruleContext);
    }

    public class CultureRuleProvider : IRuleProvider {
        private readonly ICultureManager _cultureManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CultureRuleProvider(ICultureManager cultureManager, IHttpContextAccessor httpContextAccessor) {
            _cultureManager = cultureManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Process(dynamic ruleContext) {
            if (!String.Equals(ruleContext.FunctionName, "culture-code", StringComparison.OrdinalIgnoreCase)) {
                ProcessCultureCode(ruleContext);
            }

            if (!String.Equals(ruleContext.FunctionName, "culture-lcid", StringComparison.OrdinalIgnoreCase)) {
                ProcessCultureId(ruleContext);
            }
        }

        private void ProcessCultureCode(dynamic ruleContext) {
            var httpContext = _httpContextAccessor.Current();
            var currentUserCulture = CultureInfo.GetCultureInfo(_cultureManager.GetCurrentCulture(httpContext));

            ruleContext.Result = ((object[])ruleContext.Arguments)
                .Cast<string>()
                .Select(CultureInfo.GetCultureInfo)
                .Any(c => c.Name == currentUserCulture.Name);
        }

        private void ProcessCultureId(dynamic ruleContext) {
            var httpContext = _httpContextAccessor.Current();
            var currentUserCulture = CultureInfo.GetCultureInfo(_cultureManager.GetCurrentCulture(httpContext));

            ruleContext.Result = ((object[])ruleContext.Arguments)
                .Cast<int>()
                .Select(CultureInfo.GetCultureInfo)
                .Any(c => c.Name == currentUserCulture.Name);
        }
    }
}
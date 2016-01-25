using System;
using System.Globalization;
using System.Linq;
using Orchard.Events;

namespace Orchard.Localization.RuleEngine {
    public interface IRuleProvider : IEventHandler {
        void Process(dynamic ruleContext);
    }

    public interface IConditionProvider : IEventHandler {
        void Evaluate(dynamic evaluationContext);
    }

    public class CultureRuleProvider : IRuleProvider, IConditionProvider
    {
        private readonly WorkContext _workContext;

        public CultureRuleProvider(WorkContext workContext) {
            _workContext = workContext;
        }

        public void Process(dynamic ruleContext) {
            if (String.Equals(ruleContext.FunctionName, "culturecode", StringComparison.OrdinalIgnoreCase)) {
                ProcessCultureCode(ruleContext);
            }

            if (String.Equals(ruleContext.FunctionName, "culturelcid", StringComparison.OrdinalIgnoreCase)) {
                ProcessCultureId(ruleContext);
            }

            if (String.Equals(ruleContext.FunctionName, "cultureisrtl", StringComparison.OrdinalIgnoreCase)) {
                ProcessCurrentCultureIsRtl(ruleContext);
            }

            if (String.Equals(ruleContext.FunctionName, "culturelang", StringComparison.OrdinalIgnoreCase)) {
                ProcessLanguageCode(ruleContext);
            }
        }

        public void Evaluate(dynamic evaluationContext)
        {
            if (String.Equals(evaluationContext.FunctionName, "culturecode", StringComparison.OrdinalIgnoreCase)) {
                ProcessCultureCode(evaluationContext);
            }

            if (String.Equals(evaluationContext.FunctionName, "culturelcid", StringComparison.OrdinalIgnoreCase)) {
                ProcessCultureId(evaluationContext);
            }

            if (String.Equals(evaluationContext.FunctionName, "cultureisrtl", StringComparison.OrdinalIgnoreCase)) {
                ProcessCurrentCultureIsRtl(evaluationContext);
            }

            if (String.Equals(evaluationContext.FunctionName, "culturelang", StringComparison.OrdinalIgnoreCase)) {
                ProcessLanguageCode(evaluationContext);
            }
        }

        private void ProcessCurrentCultureIsRtl(dynamic ruleContext) {
            var currentUserCulture = CultureInfo.GetCultureInfo(_workContext.CurrentCulture);

            var isRtl = ((object[]) ruleContext.Arguments)
                .Cast<bool>()
                .SingleOrDefault();

            ruleContext.Result = (isRtl == currentUserCulture.TextInfo.IsRightToLeft);
        }

        private void ProcessCultureCode(dynamic ruleContext) {
            var currentUserCulture = CultureInfo.GetCultureInfo(_workContext.CurrentCulture);

            ruleContext.Result = ((object[])ruleContext.Arguments)
                .Cast<string>()
                .Select(CultureInfo.GetCultureInfo)
                .Any(c => c.Name == currentUserCulture.Name);
        }

        private void ProcessLanguageCode(dynamic ruleContext) {
            var currentUserCulture = CultureInfo.GetCultureInfo(_workContext.CurrentCulture);

            ruleContext.Result = ((object[])ruleContext.Arguments)
                .Cast<string>()
                .Select(CultureInfo.GetCultureInfo)
                .Any(c => c.Name == currentUserCulture.TwoLetterISOLanguageName);
        }

        private void ProcessCultureId(dynamic ruleContext) {
            var currentUserCulture = CultureInfo.GetCultureInfo(_workContext.CurrentCulture);

            ruleContext.Result = ((object[])ruleContext.Arguments)
                .Cast<int>()
                .Select(CultureInfo.GetCultureInfo)
                .Any(c => c.Name == currentUserCulture.Name);
        }

        
    }
}
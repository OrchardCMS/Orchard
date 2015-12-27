using Orchard.Widgets.Services;
using System;
using Orchard.Services;
using System.Threading;

namespace Orchard.Glimpse
{
    public class DayOfTheWeekLayerRuleProvider : IRuleProvider
    {
        private readonly IClock _clock;

        public DayOfTheWeekLayerRuleProvider(IClock clock) {
            _clock = clock;
        }

        public void Process(RuleContext ruleContext)
        {
            if (!String.Equals(ruleContext.FunctionName, "DayOfTheWeek", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var dayOfTheWeek = Convert.ToString(ruleContext.Arguments[0]);

            Thread.Sleep(1000);

            ruleContext.Result = String.Equals(_clock.UtcNow.DayOfWeek.ToString(), dayOfTheWeek, StringComparison.OrdinalIgnoreCase);
        }
    }
}
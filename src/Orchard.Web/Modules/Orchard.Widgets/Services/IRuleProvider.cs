using System;
using Orchard.Events;

namespace Orchard.Widgets.Services {
    [Obsolete("Use Orchard.Conditions.Services.IConditionProvider instead.")]
    public interface IRuleProvider : IEventHandler {
        void Process(RuleContext ruleContext);
    }
}
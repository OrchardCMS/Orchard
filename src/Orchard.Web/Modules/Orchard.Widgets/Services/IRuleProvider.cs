using Orchard.Events;

namespace Orchard.Widgets.Services {
    public interface IRuleProvider : IEventHandler {
        void Process(RuleContext ruleContext);
    }
}
using Orchard.Events;

namespace Orchard.Conditions.Services {
    public interface IConditionProvider : IEventHandler {
        void Evaluate(ConditionEvaluationContext evaluationContext);
    }
}
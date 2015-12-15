using Orchard.Events;

namespace Orchard.Recipes.Events {
    public interface IRecipeSchedulerEventHandler : IEventHandler  {
        void ExecuteWork(string executionId);
    }
}

using System;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.State;
using Orchard.Logging;
using Orchard.Recipes.Events;

namespace Orchard.Recipes.Services {
    public class RecipeScheduler : IRecipeScheduler, IRecipeSchedulerEventHandler {
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly Lazy<IRecipeStepExecutor> _recipeStepExecutor;
        private readonly IShellDescriptorManagerEventHandler _events;

        public RecipeScheduler(
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager,
            Lazy<IRecipeStepExecutor> recipeStepExecutor,
            IShellDescriptorManagerEventHandler events) {
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;
            _recipeStepExecutor = recipeStepExecutor;
            _events = events;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger;

        public void ScheduleWork(string executionId) {
            var shellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            Logger.Information("Scheduling execution of recipe {0}.", executionId);
            // TODO: this task entry may need to become appdata folder backed if it isn't already
            _processingEngine.AddTask(
                _shellSettings,
                shellDescriptor,
                "IRecipeSchedulerEventHandler.ExecuteWork",
                new Dictionary<string, object> { { "executionId", executionId } });
        }

        public void ExecuteWork(string executionId) {
            Logger.Information("Executing next step of recipe {0}.", executionId);
            // todo: this callback should be guarded against concurrency by the IProcessingEngine
            var scheduleMore = _recipeStepExecutor.Value.ExecuteNextStep(executionId);
            if (scheduleMore)
                ScheduleWork(executionId);
            else
                // https://github.com/OrchardCMS/Orchard/issues/3672
                // Because recipes execute in their own workcontext, we need to restart the shell, as signaling a cache won't work across workcontexts.
                _events.Changed(_shellDescriptorManager.GetShellDescriptor(), _shellSettings.Name);
        }
    }
}
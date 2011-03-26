using System;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.State;
using Orchard.Recipes.Events;

namespace Orchard.Recipes.Services {
    public class RecipeScheduler : IRecipeScheduler, IRecipeSchedulerEventHandler {
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly Lazy<IRecipeStepExecutor> _recipeStepExecutor;

        public RecipeScheduler(
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager,
            Lazy<IRecipeStepExecutor> recipeStepExecutor) {
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;
            _recipeStepExecutor = recipeStepExecutor;
        }

        public void ScheduleWork(string executionId) {
            var shellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            // TODO: this task entry may need to become appdata folder backed if it isn't already
            _processingEngine.AddTask(
                _shellSettings,
                shellDescriptor,
                "IRecipeSchedulerEventHandler.ExecuteWork",
                new Dictionary<string, object> { { "executionId", executionId } });
        }

        public void ExecuteWork(string executionId) {
            // todo: this callback should be guarded against concurrency by the IProcessingEngine
            var scheduleMore = _recipeStepExecutor.Value.ExecuteNextStep(executionId);
            if (scheduleMore)
                ScheduleWork(executionId);
        }
    }

}
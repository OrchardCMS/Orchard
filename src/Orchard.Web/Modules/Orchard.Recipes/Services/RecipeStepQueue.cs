using System;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeStepQueue : IRecipeStepQueue {
        // placeholder to be dropped in favor of an IAppDataFolder based impl
        private readonly Queue<Tuple<RecipeStep, string>> _stepQueue;

        public RecipeStepQueue() {
            _stepQueue = new Queue<Tuple<RecipeStep, string>>();
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        public void Enqueue(string executionId, RecipeStep step) {
            _stepQueue.Enqueue(new Tuple<RecipeStep, string>(step, executionId));
        }

        public Tuple<RecipeStep, string> Dequeue(string executionId) {
            return _stepQueue.Count > 0 ? _stepQueue.Dequeue() : null;
        }
    }
}
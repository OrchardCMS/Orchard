using System.Collections.Generic;
using System.Linq;

namespace Orchard.Recipes.Models
{
    public class RecipeResult
    {
        public string ExecutionId { get; set; }
        public IEnumerable<RecipeStepResult> Steps { get; set; }
        public bool IsCompleted => Steps.All(s => s.IsCompleted);
        public bool IsSuccessful => IsCompleted && Steps.All(s => s.IsSuccessful);
    }
}
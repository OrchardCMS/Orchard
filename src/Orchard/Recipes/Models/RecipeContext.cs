using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Recipes.Models {
    public class RecipeContext {
        public RecipeStep RecipeStep { get; set; }
        public IList<FileToImport> Files { get; set; }
        public bool Executed { get; set; }
        public string ExecutionId { get; set; }
    }
}
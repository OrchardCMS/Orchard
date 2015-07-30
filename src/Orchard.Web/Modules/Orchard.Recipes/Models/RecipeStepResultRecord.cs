namespace Orchard.Recipes.Models {
    public class RecipeStepResultRecord {
        public virtual int Id { get; set; }
        public virtual string ExecutionId { get; set; }
        public virtual string RecipeName { get; set; }
        public virtual string StepId { get; set; }
        public virtual string StepName { get; set; }
        public virtual bool IsCompleted { get; set; }
        public virtual bool IsSuccessful { get; set; }
        public virtual string ErrorMessage { get; set; }
    }
}
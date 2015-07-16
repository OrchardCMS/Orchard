using System.Collections.Generic;

namespace Orchard.ImportExport.ViewModels {
    public class UploadRecipeViewModel {
        public UploadRecipeViewModel() {
            RecipeExecutionSteps = new List<RecipeExecutionStepViewModel>();
        }

        public bool ResetSite { get; set; }
        public string SuperUserName { get; set; }
        public string SuperUserPassword { get; set; }
        public string SuperUserPasswordConfirmation { get; set; }
        public IList<RecipeExecutionStepViewModel> RecipeExecutionSteps { get; set; }
    }
}
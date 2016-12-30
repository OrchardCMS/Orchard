using System.Collections.Generic;

namespace Orchard.DynamicForms.ViewModels {
    public class FormSubmissionsBuilderStepViewModel {
        public FormSubmissionsBuilderStepViewModel() {
            Submissions = new List<FormSubmissionsEntry>();
        }

        public IList<FormSubmissionsEntry> Submissions { get; set; }
    }
}
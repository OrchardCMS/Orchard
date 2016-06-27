using System.Collections.Specialized;
using Orchard.DynamicForms.Models;

namespace Orchard.DynamicForms.ViewModels {
    public class SubmissionViewModel {
        public Submission Submission { get; set; }
        public NameValueCollection NameValues { get; set; }
    }
}
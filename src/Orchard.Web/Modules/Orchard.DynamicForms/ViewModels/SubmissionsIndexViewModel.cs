using System.Data;

namespace Orchard.DynamicForms.ViewModels {
    public class SubmissionsIndexViewModel {
        public string FormName { get; set; }
        public DataTable Submissions { get; set; }
        public dynamic Pager { get; set; }
    }
}
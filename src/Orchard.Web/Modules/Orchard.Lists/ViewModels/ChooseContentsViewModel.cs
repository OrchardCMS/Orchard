using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Lists.ViewModels {
    public class ChooseContentsViewModel  {
        public ChooseContentsViewModel() {
            OrderBy = ContentsOrder.Modified;
        }

        public string FilterByContentType { get; set; }
        public int SourceContainerId { get; set; }
        public int TargetContainerId { get; set; }

        public int? Page { get; set; }

        public string SelectedFilter { get; set; }
        public IEnumerable<KeyValuePair<string, string>> FilterOptions { get; set; }
        public ContentsOrder OrderBy { get; set; }
    }

}
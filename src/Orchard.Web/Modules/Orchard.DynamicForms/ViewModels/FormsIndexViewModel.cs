using System.Collections.Generic;
using System.Linq;
using Orchard.DynamicForms.Models;

namespace Orchard.DynamicForms.ViewModels {
    public class FormsIndexViewModel {
        public IList<IGrouping<string, Submission>> Forms { get; set; }
    }
}
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Modules.ViewModels {
    public class FeatureListViewModel : BaseViewModel {
        public IEnumerable<Feature> Features { get; set; }
    }
}
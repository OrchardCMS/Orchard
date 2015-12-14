using System.Collections.Generic;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.SortCriterion;

namespace Orchard.Projections.ViewModels {
    public class SortCriterionAddViewModel {
        public int Id { get; set; }
        public IEnumerable<TypeDescriptor<SortCriterionDescriptor>> SortCriteria { get; set; }
    }
}

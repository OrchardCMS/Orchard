using System.Collections.Generic;
using Orchard.MediaProcessing.Descriptors;
using Orchard.MediaProcessing.Descriptors.Filter;

namespace Orchard.MediaProcessing.ViewModels {
    public class FilterAddViewModel {
        public int Id { get; set; }
        public IEnumerable<TypeDescriptor<FilterDescriptor>> Filters { get; set; }
    }
}

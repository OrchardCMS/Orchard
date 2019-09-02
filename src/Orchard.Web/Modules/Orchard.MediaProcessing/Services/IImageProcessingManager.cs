using System.Collections.Generic;
using Orchard.MediaProcessing.Descriptors;
using Orchard.MediaProcessing.Descriptors.Filter;

namespace Orchard.MediaProcessing.Services {
    public interface IImageProcessingManager : IDependency {
        IEnumerable<TypeDescriptor<FilterDescriptor>> DescribeFilters();
    }
}
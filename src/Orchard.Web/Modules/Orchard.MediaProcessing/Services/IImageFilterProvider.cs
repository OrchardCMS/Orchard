using Orchard.MediaProcessing.Descriptors.Filter;

namespace Orchard.MediaProcessing.Services {
    public interface IImageFilterProvider : IDependency {
        void Describe(DescribeFilterContext describe);
    }
}
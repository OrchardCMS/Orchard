using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;

namespace Orchard.MediaProcessing.Descriptors.Filter {
    public class DescribeFilterContext {
        private readonly Dictionary<string, DescribeFilterFor> _describes = new Dictionary<string, DescribeFilterFor>();

        public IEnumerable<TypeDescriptor<FilterDescriptor>> Describe() {
            return _describes.Select(kp => new TypeDescriptor<FilterDescriptor> {
                Category = kp.Key,
                Name = kp.Value.Name,
                Description = kp.Value.Description,
                Descriptors = kp.Value.Types
            });
        }

        public DescribeFilterFor For(string category) {
            return For(category, null, null);
        }

        public DescribeFilterFor For(string category, LocalizedString name, LocalizedString description) {
            DescribeFilterFor describeFor;
            if (!_describes.TryGetValue(category, out describeFor)) {
                describeFor = new DescribeFilterFor(category, name, description);
                _describes[category] = describeFor;
            }
            return describeFor;
        }
    }
}
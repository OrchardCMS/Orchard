using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;

namespace Orchard.Workflows.Models.Descriptors {
    public class DescribeActivityContext {
        private readonly Dictionary<string, DescribeActivityFor> _describes = new Dictionary<string, DescribeActivityFor>();

        public IEnumerable<TypeDescriptor<ActionDescriptor>> Describe() {
            return _describes.Select(kp => new TypeDescriptor<ActionDescriptor> {
                Category = kp.Key,
                Name = kp.Value.Name,
                Description = kp.Value.Description,
                Descriptors = kp.Value.Types
            });
        }

        public DescribeActivityFor For(string category) {
            return For(category, null, null);
        }

        public DescribeActivityFor For(string category, LocalizedString name, LocalizedString description) {
            DescribeActivityFor describeFor;
            if (!_describes.TryGetValue(category, out describeFor)) {
                describeFor = new DescribeActivityFor(category, name, description);
                _describes[category] = describeFor;
            }
            return describeFor;
        }
    }


}
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;

namespace Orchard.Rules.Models {
    public class DescribeEventContext {
        private readonly Dictionary<string, DescribeEventFor> _describes = new Dictionary<string, DescribeEventFor>();

        public IEnumerable<TypeDescriptor<EventDescriptor>> Describe() {
            return _describes.Select(kp => new TypeDescriptor<EventDescriptor> {
                Category = kp.Key,
                Name = kp.Value.Name,
                Description = kp.Value.Description,
                Descriptors = kp.Value.Types
            });
        }

        public DescribeEventFor For(string target) {
            return For(target, null, null);
        }

        public DescribeEventFor For(string category, LocalizedString name, LocalizedString description) {
            DescribeEventFor describeFor;
            if (!_describes.TryGetValue(category, out describeFor)) {
                describeFor = new DescribeEventFor(category, name, description);
                _describes[category] = describeFor;
            }
            return describeFor;
        }
    }


}
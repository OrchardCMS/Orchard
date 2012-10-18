using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;

namespace Orchard.Rules.Models {
    public class DescribeActionContext {
        private readonly Dictionary<string, DescribeActionFor> _describes = new Dictionary<string, DescribeActionFor>();

        public IEnumerable<TypeDescriptor<ActionDescriptor>> Describe() {
            return _describes.Select(kp => new TypeDescriptor<ActionDescriptor> {
                Category = kp.Key,
                Name = kp.Value.Name,
                Description = kp.Value.Description,
                Descriptors = kp.Value.Types
            });
        }

        public DescribeActionFor For(string category) {
            return For(category, null, null);
        }

        public DescribeActionFor For(string category, LocalizedString name, LocalizedString description) {
            DescribeActionFor describeFor;
            if (!_describes.TryGetValue(category, out describeFor)) {
                describeFor = new DescribeActionFor(category, name, description);
                _describes[category] = describeFor;
            }
            return describeFor;
        }
    }


}
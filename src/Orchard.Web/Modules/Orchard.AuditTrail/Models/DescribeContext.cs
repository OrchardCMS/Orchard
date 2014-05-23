using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;

namespace Orchard.AuditTrail.Models {
    public class DescribeContext {
        private readonly Dictionary<string, DescribeFor> _describes = new Dictionary<string, DescribeFor>();

        public IEnumerable<AuditTrailCategoryDescriptor> Describe() {
            var query = 
                from d in _describes.Values
                select new AuditTrailCategoryDescriptor {
                    Category = d.Category,
                    Name = d.Name,
                    Events = d.Events
                };

            return query.ToArray();
        }

        public DescribeFor For(string category, LocalizedString name) {
            DescribeFor describeFor;
            if (!_describes.TryGetValue(category, out describeFor)) {
                describeFor = new DescribeFor(category, name);
                _describes[category] = describeFor;
            }
            return describeFor;
        }
    }
}
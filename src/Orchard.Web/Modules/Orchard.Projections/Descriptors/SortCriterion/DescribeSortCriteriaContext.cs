using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;

namespace Orchard.Projections.Descriptors.SortCriterion {
    public class DescribeSortCriterionContext {
        private readonly Dictionary<string, DescribeSortCriterionFor> _describes = new Dictionary<string, DescribeSortCriterionFor>();

        public IEnumerable<TypeDescriptor<SortCriterionDescriptor>> Describe() {
            return _describes.Select(kp => new TypeDescriptor<SortCriterionDescriptor> {
                Category = kp.Key,
                Name = kp.Value.Name,
                Description = kp.Value.Description,
                Descriptors = kp.Value.Types
            });
        }

        public DescribeSortCriterionFor For(string category) {
            return For(category, null, null);
        }

        public DescribeSortCriterionFor For(string category, LocalizedString name, LocalizedString description) {
            DescribeSortCriterionFor describeFor;
            if (!_describes.TryGetValue(category, out describeFor)) {
                describeFor = new DescribeSortCriterionFor(category, name, description);
                _describes[category] = describeFor;
            }
            return describeFor;
        }
    }


}
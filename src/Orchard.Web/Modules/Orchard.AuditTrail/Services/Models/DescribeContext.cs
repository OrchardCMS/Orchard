using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;

namespace Orchard.AuditTrail.Services.Models {
    public class DescribeContext {
        private readonly IDictionary<string, DescribeFor> _describes = new Dictionary<string, DescribeFor>();
        private readonly IList<Action<QueryFilterContext>> _queryFilters = new List<Action<QueryFilterContext>>();
        private readonly IList<Action<DisplayFilterContext>> _filterDisplays = new List<Action<DisplayFilterContext>>();

        public IEnumerable<Action<QueryFilterContext>> QueryFilters {
            get { return _queryFilters; }
        }

        public IEnumerable<Action<DisplayFilterContext>> FilterDisplays {
            get { return _filterDisplays; }
        }

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

        public DescribeContext QueryFilter(Action<QueryFilterContext> queryAction) {
            _queryFilters.Add(queryAction);
            return this;
        }

        public DescribeContext DisplayFilter(Action<DisplayFilterContext> displayFilter) {
            _filterDisplays.Add(displayFilter);
            return this;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public class CombinedResult : DriverResult {
        private readonly IEnumerable<DriverResult> _results;

        public CombinedResult(IEnumerable<DriverResult> results) {
            _results = results.Where(x => x != null);
        }

        public override void Apply(BuildDisplayContext context) {
            foreach (var result in _results) {
                result.Apply(context);
            }
        }

        public override void Apply(BuildEditorContext context) {
            foreach (var result in _results) {
                result.Apply(context);
            }
        }
    }
}
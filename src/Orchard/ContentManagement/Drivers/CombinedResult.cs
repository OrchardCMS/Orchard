using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public class CombinedResult : DriverResult {
        private readonly IEnumerable<DriverResult> _results;

        public CombinedResult(IEnumerable<DriverResult> results) {
            _results = results.Where(x => x != null);
        }

        public override Task ApplyAsync(BuildDisplayContext context) {
            return Task.WhenAll(_results.Select(result =>
            {
                // copy the ContentPart which was used to render this result to its children
                // so they can assign it to the concrete shapes
                if (result.ContentPart == null && ContentPart != null)
                {
                    result.ContentPart = ContentPart;
                }

                // copy the ContentField which was used to render this result to its children
                // so they can assign it to the concrete shapes
                if (result.ContentField == null && ContentField != null)
                {
                    result.ContentField = ContentField;
                }

                return result.ApplyAsync(context);
            }).ToList());
        }

        public override Task ApplyAsync(BuildEditorContext context) {
            return Task.WhenAll(_results.Select(result => {
                // copy the ContentPart which was used to render this result to its children
                // so they can assign it to the concrete shapes
                if (result.ContentPart == null && ContentPart != null) {
                    result.ContentPart = ContentPart;
                }

                // copy the ContentField which was used to render this result to its children
                // so they can assign it to the concrete shapes
                if (result.ContentField == null && ContentField != null) {
                    result.ContentField = ContentField;
                }

                return result.ApplyAsync(context);
            }).ToList());
        }

        public IEnumerable<DriverResult> GetResults() {
            return _results;
        } 
    }
}
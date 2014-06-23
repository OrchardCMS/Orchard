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

        public override async Task ApplyAsync(BuildDisplayContext context) {
            foreach (var result in _results) {

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

                await result.ApplyAsync(context);
            }
        }

        public override async Task ApplyAsync(BuildEditorContext context) {
            foreach (var result in _results) {

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

                await result.ApplyAsync(context);
            }
        }

        public IEnumerable<DriverResult> GetResults() {
            return _results;
        } 
    }
}
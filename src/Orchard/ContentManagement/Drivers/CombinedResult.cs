using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public class CombinedResult : DriverResult {
        private readonly DriverResult[] _results;

        public CombinedResult(DriverResult[] results) {
            _results = results;
        }

        public override void Apply(BuildDisplayModelContext context) {
            foreach (var result in _results) {
                result.Apply(context);
            }
        }

        public override void Apply(BuildEditorModelContext context) {
            foreach(var result in _results) {
                result.Apply(context);
            }
        }        
    }
}
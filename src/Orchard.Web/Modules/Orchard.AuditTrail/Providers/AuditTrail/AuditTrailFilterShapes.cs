using System.IO;
using Orchard.DisplayManagement;

namespace Orchard.AuditTrail.Providers.AuditTrail {
    public class AuditTrailFilterShapes : IDependency {
        [Shape]
        public void AuditTrailFilterDisplay(dynamic Shape, dynamic Display, TextWriter Output) {
            DispayChildren(Shape, Display, Output);
        }

        private void DispayChildren(dynamic shape, dynamic display, TextWriter output) {
            foreach (var child in shape) {
                output.Write(display(child));
            }
        }
    }
}
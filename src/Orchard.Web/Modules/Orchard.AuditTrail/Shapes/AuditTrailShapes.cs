using System.IO;
using Orchard.DisplayManagement;

namespace Orchard.AuditTrail.Shapes {
    public class AuditTrailShapes : IDependency {
        [Shape]
        public void AuditTrailFilters(dynamic Shape, dynamic Display, TextWriter Output) {
            DispayChildren(Shape, Display, Output);
        }

        [Shape]
        public void AuditTrailFilters_TripleFirst(dynamic Shape, dynamic Display, TextWriter Output) {
            DispayChildren(Shape, Display, Output);
        }

        [Shape]
        public void AuditTrailFilters_TripleSecond(dynamic Shape, dynamic Display, TextWriter Output) {
            DispayChildren(Shape, Display, Output);
        }

        [Shape]
        public void AuditTrailFilters_TripleThird(dynamic Shape, dynamic Display, TextWriter Output) {
            DispayChildren(Shape, Display, Output);
        }

        private void DispayChildren(dynamic shape, dynamic display, TextWriter output) {
            foreach (var child in shape) {
                output.Write(display(child));
            }
        }
    }
}
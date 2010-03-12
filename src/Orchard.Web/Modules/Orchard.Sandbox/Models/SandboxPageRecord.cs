using Orchard.ContentManagement.Records;

namespace Orchard.Sandbox.Models {
    public class SandboxPageRecord : ContentPartRecord {
        public virtual string Name { get; set; }
    }
}
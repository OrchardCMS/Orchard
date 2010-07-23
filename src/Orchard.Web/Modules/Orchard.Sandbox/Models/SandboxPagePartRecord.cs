using Orchard.ContentManagement.Records;

namespace Orchard.Sandbox.Models {
    public class SandboxPagePartRecord : ContentPartRecord {
        public virtual string Name { get; set; }
    }
}
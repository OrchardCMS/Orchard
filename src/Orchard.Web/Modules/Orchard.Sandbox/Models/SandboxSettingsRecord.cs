using Orchard.ContentManagement.Records;

namespace Orchard.Sandbox.Models {
    public class SandboxSettingsRecord : ContentPartRecord {
        public virtual bool AllowAnonymousEdits { get; set; }
    }
}
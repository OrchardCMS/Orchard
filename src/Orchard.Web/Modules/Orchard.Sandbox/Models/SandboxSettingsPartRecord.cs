using Orchard.ContentManagement.Records;

namespace Orchard.Sandbox.Models {
    public class SandboxSettingsPartRecord : ContentPartRecord {
        public virtual bool AllowAnonymousEdits { get; set; }
    }
}
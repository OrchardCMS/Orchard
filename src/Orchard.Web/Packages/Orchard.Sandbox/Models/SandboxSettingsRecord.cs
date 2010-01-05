using System.ComponentModel.DataAnnotations;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;

namespace Orchard.Sandbox.Models {
    public class SandboxSettingsRecord : ContentPartRecord {
        public virtual bool AllowAnonymousEdits { get; set; }
    }


}

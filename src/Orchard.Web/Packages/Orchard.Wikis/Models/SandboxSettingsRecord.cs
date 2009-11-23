using System.ComponentModel.DataAnnotations;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;

namespace Orchard.Sandbox.Models {
    public class SandboxSettingsRecord : ContentPartRecord {
        public virtual bool AllowAnonymousEdits { get; set; }

        [Required]
        public virtual string NameOfThemeWhenEditingPage { get; set; }
    }


}

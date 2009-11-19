using System.ComponentModel.DataAnnotations;
using Orchard.Models.Records;

namespace Orchard.Wikis.Models {
    public class WikiSettingsRecord : ContentPartRecord {
        public virtual bool AllowAnonymousEdits { get; set; }
        
        [Required]
        public virtual string WikiEditTheme { get; set; }
    }
}
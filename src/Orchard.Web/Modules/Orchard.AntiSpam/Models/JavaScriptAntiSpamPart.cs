using Orchard.ContentManagement;

namespace Orchard.AntiSpam.Models {
    public class JavaScriptAntiSpamPart : ContentPart {
        public bool IAmHuman { get; set; }
    }
}
using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.Tests.ContentManagement.Models {
    public class Phi : ContentField {
        public Phi() {
            PartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition("Phi"), "Phi", new SettingsDictionary());
        }
    }
}

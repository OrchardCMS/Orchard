using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.Tests.ContentManagement.Models {
    public class Phi : ContentField {
        public Phi() {
            PartFieldDefinition = new ContentPartDefinition.Field(new ContentFieldDefinition("Phi"), "Phi", new Dictionary<string, string>());
        }
    }
}

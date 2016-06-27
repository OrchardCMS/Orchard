using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.DynamicForms.Services.Models {
    public class ContentFieldBindingDescriptor {
        public ContentFieldBindingDescriptor() {
            BindingContexts = new List<BindingContext>();
        }
        public ContentPartFieldDefinition Field { get; set; }
        public IList<BindingContext> BindingContexts { get; set; }
    }
}
using System.Collections.Generic;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.Core.Contents.ViewModels {
    public class ListContentTypesViewModel  {
        public IEnumerable<ContentTypeDefinition> Types { get; set; }
    }
}
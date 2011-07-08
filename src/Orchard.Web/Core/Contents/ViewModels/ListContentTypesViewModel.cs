using System.Collections.Generic;
using Orchard.ContentManagement.Definition.Models;

namespace Orchard.Core.Contents.ViewModels {
    public class ListContentTypesViewModel  {
        public IEnumerable<ContentTypeDefinition> Types { get; set; }
    }
}
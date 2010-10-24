using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.Experimental.ViewModels {

    public class MetadataIndexViewModel  {
        public IEnumerable<ContentTypeDefinition> TypeDefinitions { get; set; }
        public IEnumerable<ContentPartDefinition> PartDefinitions { get; set; }
        public string ExportText { get; set; }
    }
}
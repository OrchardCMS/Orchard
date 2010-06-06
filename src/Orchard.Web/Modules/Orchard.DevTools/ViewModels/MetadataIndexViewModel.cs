using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.DevTools.ViewModels {
    public class MetadataIndexViewModel : BaseViewModel {
        public IEnumerable<ContentTypeDefinition> TypeDefinitions { get; set; }
        public IEnumerable<ContentPartDefinition> PartDefinitions { get; set; }
        public string ExportText { get; set; }
    }
}
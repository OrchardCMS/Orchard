using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.ContentTypes.ViewModels {
    public class ListContentTypesViewModel : BaseViewModel {
        public IEnumerable<ContentTypeDefinition> Types { get; set; }
    }
}
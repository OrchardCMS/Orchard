using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.ContentTypes.ViewModels {
    public class ListContentPartsViewModel : BaseViewModel {
        public IEnumerable<ContentPartDefinition> Parts { get; set; }
    }
}
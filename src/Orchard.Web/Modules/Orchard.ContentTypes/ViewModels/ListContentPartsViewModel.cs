using System.Collections.Generic;
using Orchard.Mvc.ViewModels;

namespace Orchard.ContentTypes.ViewModels {
    public class ListContentPartsViewModel : BaseViewModel {
        public IEnumerable<EditPartViewModel> Parts { get; set; }
    }
}
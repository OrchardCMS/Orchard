using System.Collections.Generic;
using Orchard.Mvc.ViewModels;

namespace Orchard.ContentTypes.ViewModels {
    public class ListContentTypesViewModel : BaseViewModel {
        public IEnumerable<EditTypeViewModel> Types { get; set; }
    }
}
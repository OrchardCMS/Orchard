using Orchard.Mvc.ViewModels;
using Orchard.Pages.Models;

namespace Orchard.Pages.ViewModels {
    public class PageCreateViewModel : AdminViewModel {
        public ContentItemViewModel<Page> Page { get; set; }
    }
}
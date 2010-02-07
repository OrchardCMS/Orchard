using Orchard.Pages.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Pages.ViewModels {
    public class PageEditViewModel : AdminViewModel {
        public ContentItemViewModel<Page> Page { get; set; }
    }
}
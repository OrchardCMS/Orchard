using Orchard.Pages.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Pages.ViewModels {
    public class PageViewModel : BaseViewModel {
        public ContentItemViewModel<Page> Page { get; set; }
    }
}
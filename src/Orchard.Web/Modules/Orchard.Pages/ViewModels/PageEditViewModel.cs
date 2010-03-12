using Orchard.Pages.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Pages.ViewModels {
    public class PageEditViewModel : BaseViewModel {
        public ContentItemViewModel<Page> Page { get; set; }
        public bool PromoteToHomePage { get; set; }
    }
}
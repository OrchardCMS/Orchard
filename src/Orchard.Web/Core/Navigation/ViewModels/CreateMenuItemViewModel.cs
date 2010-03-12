using Orchard.Core.Navigation.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Navigation.ViewModels {
    public class CreateMenuItemViewModel : BaseViewModel {
        public ContentItemViewModel<MenuPart> MenuItem { get; set; }
    }
}
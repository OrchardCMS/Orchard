using Orchard.Mvc.ViewModels;

namespace Futures.Widgets.ViewModels {
    public class WidgetEditViewModel : BaseViewModel {
        public ContentItemViewModel Widget { get; set; }
        public string ReturnUrl { get; set;}
    }
}

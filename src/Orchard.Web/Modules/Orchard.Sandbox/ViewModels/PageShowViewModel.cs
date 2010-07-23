using Orchard.Mvc.ViewModels;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.ViewModels {
    public class PageShowViewModel : BaseViewModel {
        public ContentItemViewModel<SandboxPagePart> Page { get; set; }
    }
}

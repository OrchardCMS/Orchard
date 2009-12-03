using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.ViewModels {
    public class PageShowViewModel : BaseViewModel {
        public ItemDisplayViewModel<SandboxPage> Page { get; set; }
    }
}

using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.ViewModels {
    public class PageShowViewModel : BaseViewModel {
        public SandboxPage Page { get; set; }
        public ItemDisplayViewModel ItemView { get; set; }
    }
}

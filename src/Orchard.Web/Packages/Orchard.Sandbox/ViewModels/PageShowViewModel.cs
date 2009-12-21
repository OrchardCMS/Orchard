using Orchard.ContentManagement.ViewModels;
using Orchard.Mvc.ViewModels;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.ViewModels {
    public class PageShowViewModel : BaseViewModel {
        public ItemDisplayModel<SandboxPage> Page { get; set; }
    }
}

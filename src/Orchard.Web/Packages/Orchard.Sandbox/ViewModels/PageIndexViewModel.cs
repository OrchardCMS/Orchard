using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.ViewModels {
    public class PageIndexViewModel : BaseViewModel {
        public IList<SandboxPage> Pages { get; set; }
    } 
}

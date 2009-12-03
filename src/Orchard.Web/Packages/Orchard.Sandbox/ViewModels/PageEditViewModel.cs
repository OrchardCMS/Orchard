using System;
using System.Collections.Generic;
using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.ViewModels {
    public class PageEditViewModel : BaseViewModel {
        public ItemEditorViewModel<SandboxPage> Page { get; set; }
    }
}

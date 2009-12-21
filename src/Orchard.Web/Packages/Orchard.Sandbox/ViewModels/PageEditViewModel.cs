using System;
using System.Collections.Generic;
using Orchard.ContentManagement.ViewModels;
using Orchard.Mvc.ViewModels;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.ViewModels {
    public class PageEditViewModel : BaseViewModel {
        public ItemEditorModel<SandboxPage> Page { get; set; }
    }
}

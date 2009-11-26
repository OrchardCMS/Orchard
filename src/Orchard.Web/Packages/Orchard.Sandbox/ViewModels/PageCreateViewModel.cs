using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Mvc.ViewModels;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.ViewModels {
    public class PageCreateViewModel : BaseViewModel {
        public string Name { get; set; }
    }
}

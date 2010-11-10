using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Core.Containers.ViewModels {
    public class ContainableViewModel {
        public int ContainerId { get; set; }
        public SelectList AvailableContainers { get; set; }
    }
}

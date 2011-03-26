using System.Web.Mvc;
using Orchard.Core.Containers.Models;

namespace Orchard.Core.Containers.ViewModels {
    public class ContainerViewModel {
        public ContainerPart Part { get; set; }
        public SelectList AvailableContainables { get; set; }
    }
}
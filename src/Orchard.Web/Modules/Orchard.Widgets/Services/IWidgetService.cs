using System.Collections.Generic;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Services {
    public interface IWidgetService : IDependency {
        IEnumerable<Layer> GetLayers();
    }
}

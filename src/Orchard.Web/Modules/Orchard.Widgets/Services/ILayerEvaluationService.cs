using System.Collections.Generic;

namespace Orchard.Widgets.Services
{
    public interface ILayerEvaluationService : IDependency {
        IEnumerable<int> GetActiveLayerIds();
    }
}
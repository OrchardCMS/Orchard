using System.Collections.Generic;

namespace Orchard.Core.Common.Services {
    public interface IFlavorService : IDependency {
        IList<string> GetFlavors();
    }
}
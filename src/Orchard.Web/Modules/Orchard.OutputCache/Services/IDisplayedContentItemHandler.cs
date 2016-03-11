using System.Collections.Generic;
using Orchard;

namespace Orchard.OutputCache.Services {
    public interface IDisplayedContentItemHandler : IDependency {
        bool IsDisplayed(int id);
        IEnumerable<int> GetDisplayed();
    }
}
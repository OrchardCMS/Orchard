using System.Collections.Generic;
using Orchard;

namespace Contrib.Cache.Services {
    public interface IDisplayedContentItemHandler : IDependency {
        bool IsDisplayed(int id);
        IEnumerable<int> GetDisplayed();
    }
}
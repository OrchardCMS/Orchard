using System;
using System.Collections.Generic;
using Orchard;

namespace Futures.Modules.Packaging.Services {
    public interface IPackageSourceManager : IDependency {
        IEnumerable<PackageSource> GetSources();
        void AddSource(PackageSource source);
        void RemoveSource(Guid id);
        void UpdateLists();

        IEnumerable<PackageEntry> GetModuleList();
    }
}
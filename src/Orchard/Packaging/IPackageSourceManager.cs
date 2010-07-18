using System;
using System.Collections.Generic;

namespace Orchard.Packaging {
    public interface IPackageSourceManager : IDependency {
        IEnumerable<PackageSource> GetSources();
        void AddSource(PackageSource source);
        void RemoveSource(Guid id);
        void UpdateLists();

        IEnumerable<PackageEntry> GetModuleList();
    }
}
using System;
using System.Collections.Generic;

namespace Orchard.Packaging {
    public interface IPackagingSourceManager : IDependency {
        IEnumerable<PackagingSource> GetSources();
        void AddSource(PackagingSource source);
        void RemoveSource(Guid id);
        void UpdateLists();

        IEnumerable<PackagingEntry> GetModuleList();
    }
}
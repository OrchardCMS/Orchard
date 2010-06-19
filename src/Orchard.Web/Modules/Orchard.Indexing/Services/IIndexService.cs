using System;

namespace Orchard.Indexing.Services {
    public interface IIndexingService : IDependency {
        bool HasIndexToManage { get; }
        void RebuildIndex();
        void UpdateIndex();
        DateTime GetIndexUpdatedUtc();
    }
}
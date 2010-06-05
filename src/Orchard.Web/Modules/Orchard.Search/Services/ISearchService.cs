using System;
using System.Collections.Generic;
using Orchard.Indexing;

namespace Orchard.Search.Services {
    public interface ISearchService : IDependency {
        bool HasIndexToManage { get; }
        IEnumerable<ISearchHit> Query(string term);
        void RebuildIndex();
        void UpdateIndex();
        DateTime GetIndexUpdatedUtc();
    }
}
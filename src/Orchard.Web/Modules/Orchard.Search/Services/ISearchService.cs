using System;
using Orchard.Search.Models;

namespace Orchard.Search.Services {
    public interface ISearchService : IDependency {
        bool HasIndexToManage { get; }
        ISearchResult Query(string query, int skip, int? take);
        void RebuildIndex();
        void UpdateIndex();
        DateTime GetIndexUpdatedUtc();
    }
}
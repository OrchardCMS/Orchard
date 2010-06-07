using System;
using Orchard.Collections;
using Orchard.Indexing;

namespace Orchard.Search.Services {
    public interface ISearchService : IDependency {
        bool HasIndexToManage { get; }
        IPageOfItems<T> Query<T>(string query, int skip, int? take, Func<ISearchHit, T> shapeResult);
        void RebuildIndex();
        void UpdateIndex();
        DateTime GetIndexUpdatedUtc();
    }
}
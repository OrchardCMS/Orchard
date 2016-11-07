using System;
using Orchard.Collections;
using Orchard.Indexing;

namespace Orchard.Search.Services {
    public interface ISearchService : IDependency {
        IPageOfItems<T> Query<T>(string query, int page, int? pageSize, bool filterCulture, string index, string[] searchFields, Func<ISearchHit, T> shapeResult);
    }
}
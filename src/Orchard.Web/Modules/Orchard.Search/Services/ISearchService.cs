using System;
using Orchard.Collections;
using Orchard.Indexing;

namespace Orchard.Search.Services {
    public interface ISearchService : IDependency {
        IPageOfItems<T> Query<T>(string query, int skip, int? take, bool filterCulture, string[] searchFields, Func<ISearchHit, T> shapeResult);
    }
}
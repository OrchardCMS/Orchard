using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Orchard.Models {
    public interface IContentQuery {
        IContentManager ContentManager { get; }
        
        IContentQuery ForType(params string[] contentTypeNames);

        IContentQuery Where<TRecord>();
        IContentQuery Where<TRecord>(Expression<Func<TRecord, bool>> predicate);

        IContentQuery OrderBy<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector);
        IContentQuery OrderByDescending<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector);

        IEnumerable<ContentItem> List();
        IEnumerable<ContentItem> Slice(int skip, int count);
    }
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Orchard.Models {
    public interface IContentQuery {
        IContentManager ContentManager { get; }
        
        IContentQuery ForType(params string[] contentTypeNames);

        IContentQuery Where<TRecord>();
        IContentQuery Where<TRecord>(Expression<Func<TRecord, bool>> predicate);

        IEnumerable<ContentItem> Select();

    }
}
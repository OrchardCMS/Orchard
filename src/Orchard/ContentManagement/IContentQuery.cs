using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement {

    public interface IContentQuery {
        IContentManager ContentManager { get; }
        IContentQuery<TPart> ForPart<TPart>() where TPart : IContent;
    }

    public interface IContentQuery<TPart> : IContentQuery where TPart : IContent {
        IContentQuery<TPart> ForType(params string[] contentTypes);
        IContentQuery<TPart> ForVersion(VersionOptions options);
        IContentQuery<TPart> ForContentItems(IEnumerable<int> ids);

        IEnumerable<TPart> List();
        IEnumerable<TPart> Slice(int skip, int count);
        int Count();

        IContentQuery<TPart, TRecord> Join<TRecord>() where TRecord : ContentPartRecord;

        IContentQuery<TPart, TRecord> Where<TRecord>(Expression<Func<TRecord, bool>> predicate) where TRecord : ContentPartRecord;
        IContentQuery<TPart, TRecord> OrderBy<TRecord>(Expression<Func<TRecord, object>> keySelector) where TRecord : ContentPartRecord;
        IContentQuery<TPart, TRecord> OrderByDescending<TRecord>(Expression<Func<TRecord, object>> keySelector) where TRecord : ContentPartRecord;

        IContentQuery<TPart> WithQueryHints(QueryHints hints);
        IContentQuery<TPart> WithQueryHintsFor(string contentType);
    }

    public interface IContentQuery<TPart, TRecord> : IContentQuery<TPart> where TPart : IContent where TRecord : ContentPartRecord {
        new IContentQuery<TPart, TRecord> ForVersion(VersionOptions options);

        IContentQuery<TPart, TRecord> Where(Expression<Func<TRecord, bool>> predicate);
        IContentQuery<TPart, TRecord> OrderBy<TKey>(Expression<Func<TRecord, TKey>> keySelector);
        IContentQuery<TPart, TRecord> OrderByDescending<TKey>(Expression<Func<TRecord, TKey>> keySelector);

        new IContentQuery<TPart, TRecord> WithQueryHints(QueryHints hints);
        new IContentQuery<TPart, TRecord> WithQueryHintsFor(string contentType);
    }
}
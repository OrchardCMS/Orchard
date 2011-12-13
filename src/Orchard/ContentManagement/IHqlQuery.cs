using System;
using System.Collections.Generic;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement {

    public interface IHqlQuery {
        IContentManager ContentManager { get; }
        IHqlQuery ForType(params string[] contentTypes);
        IHqlQuery ForVersion(VersionOptions options);
        IHqlQuery<T> ForPart<T>() where T : IContent;

        IEnumerable<ContentItem> List();
        IEnumerable<ContentItem> Slice(int skip, int count);
        int Count();

        IHqlQuery Join(Action<IAliasFactory> alias);
        IHqlQuery Where(Action<IAliasFactory> alias, Action<IHqlExpressionFactory> predicate);
        IHqlQuery OrderBy(Action<IAliasFactory> alias, Action<IHqlSortFactory> order);
    }

    public interface IHqlQuery<out TPart> where TPart : IContent {
        IHqlQuery<TPart> ForType(params string[] contentTypes);
        IHqlQuery<TPart> ForVersion(VersionOptions options);

        IEnumerable<TPart> List();
        IEnumerable<TPart> Slice(int skip, int count);
        int Count();

        IHqlQuery<TPart> Join(Action<IAliasFactory> alias);
        IHqlQuery<TPart> Where(Action<IAliasFactory> alias, Action<IHqlExpressionFactory> predicate);
        IHqlQuery<TPart> OrderBy(Action<IAliasFactory> alias, Action<IHqlSortFactory> order);
    }


    public interface IAlias {
        string Name { get; }
    }

    public interface IAliasFactory {
        IAliasFactory ContentPartRecord<TRecord>() where TRecord : ContentPartRecord;
        IAliasFactory ContentPartRecord(Type contentPartRecord);
        IAliasFactory Property(string propertyName, string alias);
        IAliasFactory Named(string alias); // returns an existing alias
    }

    public interface IHqlSortFactory {
        void Asc(string propertyName);
        void Desc(string propertyName);
    }
}


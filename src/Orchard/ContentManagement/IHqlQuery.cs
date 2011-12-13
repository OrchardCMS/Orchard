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

    /*
     *
     *  query                                       // IHqlQuery<ContentItem>
     *      .Join<RoutePartRecord>().As("route")                // IAlias(query), IAlias<T>(query<TP>), IAlias<TP,TR>(query<TP,TR>) alias name is implicit, there can't be any conflict
     *          x => x.Where(route => route.
     *      )
     * 
     * query.Join<
     * 
     * query: IAlias
     * 
     * 
     * IHqlQuery<T> ONLY, because we can have <ContentItem> by default, and the Records can't be used in expression though, thus having them
     * in the generic type is useless. _manager.HqlQuery<GammaPart>() will create a new query then apply ForPart<GammaPart>
     * 
     * .Join<TRecord> is only valid for ContentPartRecord because they will resolve the fake civ.{TRecord} property
     * .Where<TRecord> is only valid for ContentPartRecord because they will resolve the fake civ.{TRecord} property
     * .Where( on => on.Named("foo"), x => x.Eq("Id", 1))
     * 
     * 
     * .Join( on => on.ContentPartRecord<FieldIndexPartRecord>().Property("IntegerFieldIndexRecords", alias: "integerFields")
     * .Where( on => on.Named("foo"), x => x.Eq("Id", 1))
     * .Where( on => on.ContentPartRecord<FieldIndexPartRecord>(), x => x.Eq("Id", 1))
     * 
     * 
     * Join(Action<IAliasFactory> alias)
     * Where(Action<IAliasFactory> alias, Action<IHqlExpression> predicate)
     * 
     * Thus we can create aliases directly from the Where()
     * 
     * IAlias {
     *   ContentPartRecord<TRecord>() where TRecord : ContentPartRecord
     *   Property(string propertyName, string alias)
     *   Named(string alias) // returns an existing alias
     *   
     * }
     */


}


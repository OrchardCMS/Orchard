using System;
using System.Collections.Generic;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement {

    /// <summary>
    /// Reprensents dynamically created query on Content Items.
    /// </summary>
    public interface IHqlQuery {

        /// <summary>
        /// The underlying <see cref="ContentManager"/> instance used to execute the query.
        /// </summary>
        IContentManager ContentManager { get; }

        /// <summary>
        /// Add content type constraints to the query.
        /// </summary>
        IHqlQuery ForType(params string[] contentTypes);
        
        /// <summary>
        /// Adds versioning options to the query.
        /// </summary>
        IHqlQuery ForVersion(VersionOptions options);

        /// <summary>
        /// Defines the resulting type.
        /// </summary>
        /// <returns>An <see cref="IHqlQuery&lt;T&gt;"/></returns>
        IHqlQuery<T> ForPart<T>() where T : IContent;

        /// <summary>
        /// Executes the query and returns all results.
        /// </summary>
        IEnumerable<ContentItem> List();

        /// <summary>
        /// Returns a subset of the matching content items.
        /// </summary>
        IEnumerable<ContentItem> Slice(int skip, int count);

        /// <summary>
        /// Returns the number of matching content items.
        /// </summary>
        int Count();

        /// <summary>
        /// Adds a join to a the query.
        /// </summary>
        /// <param name="alias">An expression pointing to the joined relationship.</param>
        IHqlQuery Join(Action<IAliasFactory> alias);

        /// <summary>
        /// Adds a where constraint to the query.
        /// </summary>
        /// <param name="alias">An expression pointing to the joined relationship.</param>
        /// <param name="predicate">A predicate expression.</param>
        IHqlQuery Where(Action<IAliasFactory> alias, Action<IHqlExpressionFactory> predicate);

        /// <summary>
        /// Adds a join to a specific relationship.
        /// </summary>
        /// <param name="alias">An expression pointing to the joined relationship.</param>
        /// <param name="order">An order expression.</param>
        IHqlQuery OrderBy(Action<IAliasFactory> alias, Action<IHqlSortFactory> order);
    }

    /// <summary>
    /// Reprensents dynamically created query on Content Items, having a specific Content Part.
    /// </summary>
    public interface IHqlQuery<TPart> where TPart : IContent {

        /// <summary>
        /// Add content type constraints to the query.
        /// </summary>
        IHqlQuery<TPart> ForType(params string[] contentTypes);

        /// <summary>
        /// Adds versioning options to the query.
        /// </summary>
        IHqlQuery<TPart> ForVersion(VersionOptions options);

        /// <summary>
        /// Executes the query and returns all results.
        /// </summary>
        IEnumerable<TPart> List();

        /// <summary>
        /// Returns a subset of the matching content items.
        /// </summary>
        IEnumerable<TPart> Slice(int skip, int count);

        /// <summary>
        /// Returns the number of matching content items.
        /// </summary>
        int Count();

        /// <summary>
        /// Adds a join to a the query.
        /// </summary>
        /// <param name="alias">An expression pointing to the joined relationship.</param>
        IHqlQuery<TPart> Join(Action<IAliasFactory> alias);

        /// <summary>
        /// Adds a where constraint to the query.
        /// </summary>
        /// <param name="alias">An expression pointing to the joined relationship.</param>
        /// <param name="predicate">A predicate expression.</param>
        IHqlQuery<TPart> Where(Action<IAliasFactory> alias, Action<IHqlExpressionFactory> predicate);

        /// <summary>
        /// Adds a join to a specific relationship.
        /// </summary>
        /// <param name="alias">An expression pointing to the joined relationship.</param>
        /// <param name="order">An order expression.</param>
        IHqlQuery<TPart> OrderBy(Action<IAliasFactory> alias, Action<IHqlSortFactory> order);
    }

    public interface IAlias {

        /// <summary>
        /// The name of the alias.
        /// </summary>
        string Name { get; }
    }

    public interface IAliasFactory {
        /// <summary>
        /// Creates a join on a content part record or returns it if it already exists.
        /// </summary>
        IAliasFactory ContentPartRecord<TRecord>() where TRecord : ContentPartRecord;
        
        /// <summary>
        /// Creates a join on a content part record or returns it if it already exists.
        /// </summary>
        IAliasFactory ContentPartRecord(Type contentPartRecord);

        /// <summary>
        /// Creates a join based on a property, or returns it if it already exists.
        /// </summary>
        IAliasFactory Property(string propertyName, string alias);
        
        /// <summary>
        /// Returns an existing alias by its name.
        /// </summary>
        IAliasFactory Named(string alias);

        /// <summary>
        /// Returns an the <see cref="ContentItemRecord"/> alias.
        /// </summary>
        IAliasFactory ContentItem();

        /// <summary>
        /// Returns an the <see cref="ContentItemVersionRecord"/> alias.
        /// </summary>
        IAliasFactory ContentItemVersion();

        /// <summary>
        /// Returns an the <see cref="ContentTypeRecord"/> alias.
        /// </summary>
        IAliasFactory ContentType();
    }

    public interface IHqlSortFactory {
        /// <summary>
        /// Sorts by ascending order
        /// </summary>
        void Asc(string propertyName);

        /// <summary>
        /// Sorts by descending order
        /// </summary>
        void Desc(string propertyName);

        /// <summary>
        /// Sorts randomly
        /// </summary>
        void Random();
    }
}


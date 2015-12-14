using System;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Projections.FilterEditors {
    /// <summary>
    /// Defines a service to provide filters.
    /// An implementation is responsible for returning a specific Form, and return a predicate.
    /// </summary>
    public interface IFilterEditor : IDependency {

        /// <summary>
        /// Whether this instance can handle a given storage type
        /// </summary>
        bool CanHandle(Type type);

        /// <summary>
        /// The name of the form which will represent this editor
        /// </summary>
        string FormName { get; }

        /// <summary>
        /// Returns a predicate representing the filter
        /// </summary>
        Action<IHqlExpressionFactory> Filter(string property, dynamic formState);

        /// <summary>
        /// Returns a textual description of a filter
        /// </summary>
        LocalizedString Display(string property, dynamic formState);

    }
}
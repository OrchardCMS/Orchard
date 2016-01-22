using System;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Projections.FilterEditors {
    /// <summary>
    /// Coordinated all available <see cref="IFilterEditor"/> to apply specific formatting on a model binding property
    /// </summary>
    public interface IFilterCoordinator : IDependency {
        
        /// <summary>
        /// Returns the form for a specific type
        /// </summary>
        string GetForm(Type type);

        /// <summary>
        /// Returns a predicate representing the filter for a specific type
        /// </summary>
        Action<IHqlExpressionFactory> Filter(Type type, string property, dynamic formState);

        /// <summary>
        /// Returns a textual description of a filter
        /// </summary>
        LocalizedString Display(Type type, string property, dynamic formState);
    }
}
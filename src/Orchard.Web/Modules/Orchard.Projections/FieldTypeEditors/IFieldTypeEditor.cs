using System;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Projections.FieldTypeEditors {
    /// <summary>
    /// Defines a service to provide information on how a specific data type
    /// is stored in the content fields index.
    /// </summary>
    public interface IFieldTypeEditor : IDependency {
        /// <summary>
        /// Whether this instance can handle a given storage type
        /// </summary>
        bool CanHandle(Type storageType);

        /// <summary>
        /// The name of the form which will represent this editor
        /// </summary>
        string FormName { get; }

        /// <summary>
        /// Generates a predicate based on the values which were provided
        /// by the user to the editor form
        /// </summary>
        Action<IHqlExpressionFactory> GetFilterPredicate(dynamic formState);

        /// <summary>
        /// Generates the textual representation of the filter
        /// </summary>
        LocalizedString DisplayFilter(string fieldName, string storageName, dynamic formState);

        /// <summary>
        /// Defines the relationship to the corresponding field indexing table for this editor
        /// </summary>
        Action<IAliasFactory> GetFilterRelationship(string aliasName);

    }

}

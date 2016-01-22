﻿using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Modules.Models;

namespace Orchard.Modules.Services {
    public interface IModuleService : IDependency {
        /// <summary>
        /// Retrieves an enumeration of the available features together with its state (enabled / disabled).
        /// </summary>
        /// <returns>An enumeration of the available features together with its state (enabled / disabled).</returns>
        IEnumerable<ModuleFeature> GetAvailableFeatures();

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        void EnableFeatures(IEnumerable<string> featureIds);

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        void EnableFeatures(IEnumerable<string> featureIds, bool force);

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        void DisableFeatures(IEnumerable<string> featureIds);

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        void DisableFeatures(IEnumerable<string> featureIds, bool force);

        /// <summary>
        /// Determines if an extension was recently installed.
        /// </summary>
        /// <param name="extensionDescriptor">The extension descriptor.</param>
        /// <returns>True if the feature was recently installed; false otherwise.</returns>
        bool IsRecentlyInstalled(ExtensionDescriptor extensionDescriptor);

        /// <summary>
        /// Gets a list of dependent features of a given feature.
        /// </summary>
        /// <param name="featureId">ID of a feature.</param>
        /// <returns>List of dependent feature descriptors.</returns>
        IEnumerable<FeatureDescriptor> GetDependentFeatures(string featureId);
    }
}
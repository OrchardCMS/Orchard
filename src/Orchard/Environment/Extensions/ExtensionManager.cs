using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Caching;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Utility;
using Orchard.Utility.Extensions;

namespace Orchard.Environment.Extensions {
    public class ExtensionManager : IExtensionManager {
        private readonly IEnumerable<IExtensionFolders> _folders;
        private readonly ICacheManager _cacheManager;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private IEnumerable<FeatureDescriptor> _featureDescriptors;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ExtensionManager(IEnumerable<IExtensionFolders> folders, IEnumerable<IExtensionLoader> loaders, ICacheManager cacheManager) {
            _folders = folders;
            _cacheManager = cacheManager;
            _loaders = loaders.OrderBy(x => x.Order).ToArray();
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        // This method does not load extension types, simply parses extension manifests from 
        // the filesystem. 
        public ExtensionDescriptor GetExtension(string name) {
            return AvailableExtensions().FirstOrDefault(x => x.Name == name);
        }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _folders.SelectMany(folder => folder.AvailableExtensions());
        }

        public IEnumerable<FeatureDescriptor> AvailableFeatures() {
            if (_featureDescriptors == null || _featureDescriptors.Count() == 0) {
                _featureDescriptors = AvailableExtensions().SelectMany(ext => ext.Features).OrderByDependencies(HasDependency).ToReadOnlyCollection();
                return _featureDescriptors;
            }
            return _featureDescriptors;
        }

        /// <summary>
        /// Returns true if the item has an explicit or implicit dependency on the subject
        /// </summary>
        /// <param name="item"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        internal static bool HasDependency(FeatureDescriptor item, FeatureDescriptor subject) {
            if (item.Extension.ExtensionType == "Theme") {
                // Themes implicitly depend on modules to ensure build and override ordering
                if (subject.Extension.ExtensionType == "Module") {
                    return true;
                }
                if (subject.Extension.ExtensionType == "Theme") {
                    // theme depends on another if it is its base theme
                    return item.Extension.BaseTheme == subject.Name;
                }
            }

            // Return based on explicit dependencies
            return item.Dependencies != null &&
                   item.Dependencies.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x, subject.Name));
        }

        public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
            return featureDescriptors
                .Select(descriptor => _cacheManager.Get(descriptor.Name, ctx => LoadFeature(descriptor)))
                .ToArray();
        }

        private Feature LoadFeature(FeatureDescriptor featureDescriptor) {
            
            var extensionDescriptor = featureDescriptor.Extension;
            var featureName = featureDescriptor.Name;
            var extensionName = extensionDescriptor.Name;

            var extensionEntry = _cacheManager.Get(extensionName, ctx => BuildEntry(extensionDescriptor));
            if (extensionEntry == null) {
                // If the feature could not be compiled for some reason,
                // return a "null" feature, i.e. a feature with no exported types.
                return new Feature {
                    Descriptor = featureDescriptor,
                    ExportedTypes = Enumerable.Empty<Type>()
                };
            }

            var extensionTypes = extensionEntry.ExportedTypes.Where(t => t.IsClass && !t.IsAbstract);
            var featureTypes = new List<Type>();

            foreach (var type in extensionTypes) {
                string sourceFeature = GetSourceFeatureNameForType(type, extensionName);
                if (String.Equals(sourceFeature, featureName, StringComparison.OrdinalIgnoreCase)) {
                    featureTypes.Add(type);
                }
            }

            return new Feature {
                Descriptor = featureDescriptor,
                ExportedTypes = featureTypes
            };
        }

        private static string GetSourceFeatureNameForType(Type type, string extensionName) {
            foreach (OrchardFeatureAttribute featureAttribute in type.GetCustomAttributes(typeof(OrchardFeatureAttribute), false)) {
                return featureAttribute.FeatureName;
            }
            return extensionName;
        }
        
        private ExtensionEntry BuildEntry(ExtensionDescriptor descriptor) {
            foreach (var loader in _loaders) {
                ExtensionEntry entry = loader.Load(descriptor);
                if (entry != null)
                    return entry;
            }

            Logger.Warning("No suitable loader found for extension \"{0}\"", descriptor.Name);
            return null;
        }
    }
}
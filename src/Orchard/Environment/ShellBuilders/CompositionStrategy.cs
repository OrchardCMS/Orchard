using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Autofac.Core;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Environment.ShellBuilders {
    public class CompositionStrategy : ICompositionStrategy {
        private readonly IExtensionManager _extensionManager;

        public CompositionStrategy(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ShellBlueprint Compose(ShellSettings settings, ShellDescriptor descriptor) {
            Logger.Debug("Composing blueprint");

            var builtinFeatures = BuiltinFeatures().ToList();
            var builtinFeatureDescriptors = builtinFeatures.Select(x => x.Descriptor).ToList();
            var availableFeatures = _extensionManager.AvailableFeatures()
                .Concat(builtinFeatureDescriptors)
                .GroupBy(x => x.Id.ToLowerInvariant()) // prevent duplicates
                .Select(x => x.FirstOrDefault())
                .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
            var enabledFeatures = _extensionManager.EnabledFeatures(descriptor).Select(x => x.Id).ToList();
            var expandedFeatures = ExpandDependencies(availableFeatures, descriptor.Features.Select(x => x.Name)).ToList();
            var autoEnabledDependencyFeatures = expandedFeatures.Except(enabledFeatures).Except(builtinFeatureDescriptors.Select(x => x.Id)).ToList();
            var featureDescriptors = _extensionManager.EnabledFeatures(expandedFeatures.Select(x => new ShellFeature { Name = x})).ToList();
            var features = _extensionManager.LoadFeatures(featureDescriptors);

            if (descriptor.Features.Any(feature => feature.Name == "Orchard.Framework"))
                features = builtinFeatures.Concat(features);

            var excludedTypes = GetExcludedTypes(features);
            var modules = BuildBlueprint(features, IsModule, BuildModule, excludedTypes);
            var dependencies = BuildBlueprint(features, IsDependency, (t, f) => BuildDependency(t, f, descriptor), excludedTypes);
            var controllers = BuildBlueprint(features, IsController, BuildController, excludedTypes);
            var httpControllers = BuildBlueprint(features, IsHttpController, BuildController, excludedTypes);
            var records = BuildBlueprint(features, IsRecord, (t, f) => BuildRecord(t, f, settings), excludedTypes);

            var result = new ShellBlueprint {
                Settings = settings,
                Descriptor = descriptor,
                Dependencies = dependencies.Concat(modules).ToArray(),
                Controllers = controllers,
                HttpControllers = httpControllers,
                Records = records,
            };

            Logger.Debug("Done composing blueprint.");

            if (autoEnabledDependencyFeatures.Any()) {
                // Add any dependencies previously not enabled to the shell descriptor.
                descriptor.Features = descriptor.Features.Concat(autoEnabledDependencyFeatures.Select(x => new ShellFeature { Name = x })).ToList();
                Logger.Information("Automatically enabled the following dependency features: {0}.", String.Join(", ", autoEnabledDependencyFeatures));
            }

            return result;
        }

        private IEnumerable<string> ExpandDependencies(IDictionary<string, FeatureDescriptor> availableFeatures, IEnumerable<string> features) {
            return ExpandDependenciesInternal(availableFeatures, features, dependentFeatureDescriptor: null).Distinct();
        }

        private IEnumerable<string> ExpandDependenciesInternal(IDictionary<string, FeatureDescriptor> availableFeatures, IEnumerable<string> features, FeatureDescriptor dependentFeatureDescriptor = null) {
            foreach (var shellFeature in features) {

                if (!availableFeatures.ContainsKey(shellFeature)) {
                    // If the feature comes from a list of feature dependencies it indicates a bug, so throw an exception.
                    if(dependentFeatureDescriptor != null)
                        throw new OrchardException(
                            T("The feature '{0}' was listed as a dependency of '{1}' of extension '{2}', but this feature could not be found. Please update your manifest file or install the module providing the missing feature.",
                            shellFeature,
                            dependentFeatureDescriptor.Name,
                            dependentFeatureDescriptor.Extension.Name));

                    // If the feature comes from the shell descriptor it means the feature is simply orphaned, so don't throw an exception.
                    Logger.Warning("Identified '{0}' as an orphaned feature state record in Settings_ShellFeatureRecord.", shellFeature);
                    continue;
                }

                var feature = availableFeatures[shellFeature];
                
                foreach (var childDependency in ExpandDependenciesInternal(availableFeatures, feature.Dependencies, dependentFeatureDescriptor: feature))
                    yield return childDependency;

                foreach (var dependency in feature.Dependencies)
                    yield return dependency;

                yield return shellFeature;
            }
        }

        private static IEnumerable<string> GetExcludedTypes(IEnumerable<Feature> features) {
            var excludedTypes = new HashSet<string>();

            // Identify replaced types.
            foreach (var feature in features) {
                foreach (var type in feature.ExportedTypes) {
                    foreach (OrchardSuppressDependencyAttribute replacedType in type.GetCustomAttributes(typeof(OrchardSuppressDependencyAttribute), false)) {
                        excludedTypes.Add(replacedType.FullName);
                    }
                }
            }

            return excludedTypes;
        }

        private static IEnumerable<Feature> BuiltinFeatures() {
            yield return new Feature {
                Descriptor = new FeatureDescriptor {
                    Id = "Orchard.Framework",
                    Extension = new ExtensionDescriptor {
                        Id = "Orchard.Framework"
                    }
                },
                ExportedTypes =
                    typeof(OrchardStarter).Assembly.GetExportedTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Except(new[] { typeof(DefaultOrchardHost) })
                    .ToArray()
            };
        }

        private static IEnumerable<T> BuildBlueprint<T>(
            IEnumerable<Feature> features,
            Func<Type, bool> predicate,
            Func<Type, Feature, T> selector,
            IEnumerable<string> excludedTypes ) {

            // Load types excluding the replaced types
            return features.SelectMany(
                feature => feature.ExportedTypes
                               .Where(predicate)
                               .Where(type => !excludedTypes.Contains(type.FullName))
                               .Select(type => selector(type, feature)))
                .ToArray();
        }

        private static bool IsModule(Type type) {
            return typeof(IModule).IsAssignableFrom(type);
        }

        private static DependencyBlueprint BuildModule(Type type, Feature feature) {
            return new DependencyBlueprint { Type = type, Feature = feature, Parameters = Enumerable.Empty<ShellParameter>() };
        }

        private static bool IsDependency(Type type) {
            return typeof(IDependency).IsAssignableFrom(type);
        }

        private static DependencyBlueprint BuildDependency(Type type, Feature feature, ShellDescriptor descriptor) {
            return new DependencyBlueprint {
                Type = type,
                Feature = feature,
                Parameters = descriptor.Parameters.Where(x => x.Component == type.FullName).ToArray()
            };
        }

        private static bool IsController(Type type) {
            return typeof(IController).IsAssignableFrom(type);
        }

        private static bool IsHttpController(Type type) {
            return typeof(IHttpController).IsAssignableFrom(type);
        }

        private static ControllerBlueprint BuildController(Type type, Feature feature) {
            var areaName = feature.Descriptor.Extension.Id;

            var controllerName = type.Name;
            if (controllerName.EndsWith("Controller"))
                controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);

            return new ControllerBlueprint {
                Type = type,
                Feature = feature,
                AreaName = areaName,
                ControllerName = controllerName,
            };
        }

        private static bool IsRecord(Type type) {
            return ((type.Namespace ?? "").EndsWith(".Models") || (type.Namespace ?? "").EndsWith(".Records")) &&
                   type.GetProperty("Id") != null &&
                   (type.GetProperty("Id").GetAccessors()).All(x => x.IsVirtual) &&
                   !type.IsSealed &&
                   !type.IsAbstract &&
                   (!typeof(IContent).IsAssignableFrom(type) || typeof(ContentPartRecord).IsAssignableFrom(type));
        }

        private static RecordBlueprint BuildRecord(Type type, Feature feature, ShellSettings settings) {
            var extensionDescriptor = feature.Descriptor.Extension;
            var extensionName = extensionDescriptor.Id.Replace('.', '_');

            var dataTablePrefix = "";
            if (!string.IsNullOrEmpty(settings.DataTablePrefix))
                dataTablePrefix = settings.DataTablePrefix + "_";

            return new RecordBlueprint {
                Type = type,
                Feature = feature,
                TableName = dataTablePrefix + extensionName + '_' + type.Name,
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Metadata;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Logging;
using Orchard.Utility;
using Orchard.Utility.Extensions;

namespace Orchard.DisplayManagement.Descriptors {

    public class DefaultShapeTableManager : IShapeTableManager {
        private readonly IEnumerable<Meta<IShapeTableProvider>> _bindingStrategies;
        private readonly IExtensionManager _extensionManager;
        private readonly ICacheManager _cacheManager;
        private readonly IParallelCacheContext _parallelCacheContext;
        private readonly Work<IEnumerable<IShapeTableEventHandler>> _shapeTableEventHandlersWork;

        public DefaultShapeTableManager(
            IEnumerable<Meta<IShapeTableProvider>> bindingStrategies,
            IExtensionManager extensionManager,
            ICacheManager cacheManager,
            IParallelCacheContext parallelCacheContext,
            Work<IEnumerable<IShapeTableEventHandler>> shapeTableEventHandlersWork
            ) {
            _extensionManager = extensionManager;
            _cacheManager = cacheManager;
            _parallelCacheContext = parallelCacheContext;
            _shapeTableEventHandlersWork = shapeTableEventHandlersWork;
            _bindingStrategies = bindingStrategies;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ShapeTable GetShapeTable(string themeName) {
            return _cacheManager.Get(themeName ?? "", true, x => {
                Logger.Information("Start building shape table");

                var alterationSets = _parallelCacheContext.RunInParallel(_bindingStrategies, bindingStrategy => {
                    Feature strategyDefaultFeature = bindingStrategy.Metadata.ContainsKey("Feature") ?
                                                               (Feature)bindingStrategy.Metadata["Feature"] :
                                                               null;

                    var builder = new ShapeTableBuilder(strategyDefaultFeature);
                    bindingStrategy.Value.Discover(builder);
                    return builder.BuildAlterations().ToReadOnlyCollection();
                });

                var alterations = alterationSets
                .SelectMany(shapeAlterations => shapeAlterations)
                .Where(alteration => IsModuleOrRequestedTheme(alteration, themeName))
                .OrderByDependenciesAndPriorities(AlterationHasDependency, GetPriority)
                .ToList();

                var descriptors = alterations.GroupBy(alteration => alteration.ShapeType, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.Aggregate(
                        new ShapeDescriptor { ShapeType = group.Key },
                        (descriptor, alteration) => {
                            alteration.Alter(descriptor);
                            return descriptor;
                        })).ToList();

                foreach(var descriptor in descriptors) {
                    foreach(var alteration in alterations.Where(a => a.ShapeType == descriptor.ShapeType).ToList()) {
                        var local = new ShapeDescriptor { ShapeType = descriptor.ShapeType };
                        alteration.Alter(local);
                        descriptor.BindingSources.Add(local.BindingSource);
                    }
                }

                var result = new ShapeTable {
                    Descriptors = descriptors.ToDictionary(sd => sd.ShapeType, StringComparer.OrdinalIgnoreCase),
                    Bindings = descriptors.SelectMany(sd => sd.Bindings).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase),
                };

                _shapeTableEventHandlersWork.Value.Invoke(ctx => ctx.ShapeTableCreated(result), Logger);

                Logger.Information("Done building shape table");
                return result;
            });
        }

        private static int GetPriority(ShapeAlteration shapeAlteration) {
            return shapeAlteration.Feature.Descriptor.Priority;
        }

        private static bool AlterationHasDependency(ShapeAlteration item, ShapeAlteration subject) {
            return ExtensionManager.HasDependency(item.Feature.Descriptor, subject.Feature.Descriptor);
        }

        private bool IsModuleOrRequestedTheme(ShapeAlteration alteration, string themeName) {
            if (alteration == null ||
                alteration.Feature == null ||
                alteration.Feature.Descriptor == null ||
                alteration.Feature.Descriptor.Extension == null) {
                return false;
            }

            var extensionType = alteration.Feature.Descriptor.Extension.ExtensionType;
            if (DefaultExtensionTypes.IsModule(extensionType)) {
                return true;
            }

            if (DefaultExtensionTypes.IsTheme(extensionType)) {
                // alterations from themes must be from the given theme or a base theme
                var featureName = alteration.Feature.Descriptor.Id;
                return String.IsNullOrEmpty(featureName) || featureName == themeName || IsBaseTheme(featureName, themeName);
            }

            return false;
        }

        private bool IsBaseTheme(string featureName, string themeName) {
            // determine if the given feature is a base theme of the given theme
            var availableFeatures = _extensionManager.AvailableFeatures();

            var themeFeature = availableFeatures.SingleOrDefault(fd => fd.Id == themeName);
            while (themeFeature != null) {
                var baseTheme = themeFeature.Extension.BaseTheme;
                if (String.IsNullOrEmpty(baseTheme)) {
                    return false;
                }
                if (featureName == baseTheme) {
                    return true;
                }
                themeFeature = availableFeatures.SingleOrDefault(fd => fd.Id == baseTheme);
            }
            return false;
        }
    }
}

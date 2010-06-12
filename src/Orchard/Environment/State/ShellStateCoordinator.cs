using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.State.Models;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;

namespace Orchard.Environment.State {
    public class ShellStateCoordinator : IShellStateManagerEventHandler, IShellDescriptorManagerEventHandler {
        private readonly ShellSettings _settings;
        private readonly IShellStateManager _stateManager;
        private readonly IExtensionManager _extensionManager;
        private readonly IProcessingEngine _processingEngine;
        private readonly IFeatureEventHandler _featureEvents;

        public ShellStateCoordinator(
            ShellSettings settings,
            IShellStateManager stateManager,
            IExtensionManager extensionManager,
            IProcessingEngine processingEngine,
            IFeatureEventHandler featureEvents) {
            _settings = settings;
            _stateManager = stateManager;
            _extensionManager = extensionManager;
            _processingEngine = processingEngine;
            _featureEvents = featureEvents;
        }

        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor) {
            // deduce and apply state changes involved
            var shellState = _stateManager.GetShellState();
            foreach (var feature in descriptor.Features) {
                var featureName = feature.Name;
                var featureState = shellState.Features.SingleOrDefault(f => f.Name == featureName);
                if (featureState == null) {
                    featureState = new ShellFeatureState {
                        Name = featureName
                    };
                    shellState.Features = shellState.Features.Concat(new[] { featureState });
                }
                if (!featureState.IsInstalled) {
                    _stateManager.UpdateInstalledState(featureState, ShellFeatureState.State.Rising);
                }
                if (!featureState.IsEnabled) {
                    _stateManager.UpdateEnabledState(featureState, ShellFeatureState.State.Rising);
                }
            }
            foreach (var featureState in shellState.Features) {
                var featureName = featureState.Name;
                if (descriptor.Features.Any(f => f.Name == featureName)) {
                    continue;
                }
                if (!featureState.IsDisabled) {
                    _stateManager.UpdateEnabledState(featureState, ShellFeatureState.State.Falling);
                }
            }

            FireApplyChangesIfNeeded();
        }

        private void FireApplyChangesIfNeeded() {
            var shellState = _stateManager.GetShellState();
            if (shellState.Features.Any(FeatureIsChanging)) {
                var descriptor = new ShellDescriptor {
                    Features = shellState.Features
                        .Where(FeatureShouldBeLoadedForStateChangeNotifications)
                        .Select(x => new ShellFeature {
                            Name = x.Name
                        })
                        .ToArray()
                };

                _processingEngine.AddTask(
                    _settings,
                    descriptor,
                    "IShellStateManagerEventHandler.ApplyChanges",
                    new Dictionary<string, object>());
            }
        }

        private static bool FeatureIsChanging(ShellFeatureState shellFeatureState) {
            if (shellFeatureState.EnableState == ShellFeatureState.State.Rising ||
                shellFeatureState.EnableState == ShellFeatureState.State.Falling) {
                return true;
            }
            if (shellFeatureState.InstallState == ShellFeatureState.State.Rising ||
                shellFeatureState.InstallState == ShellFeatureState.State.Falling) {
                return true;
            }
            return false;
        }

        private static bool FeatureShouldBeLoadedForStateChangeNotifications(ShellFeatureState shellFeatureState) {
            return FeatureIsChanging(shellFeatureState) || shellFeatureState.EnableState == ShellFeatureState.State.Up;
        }

        void IShellStateManagerEventHandler.ApplyChanges() {
            var shellState = _stateManager.GetShellState();

            // start with description of all declared features in order - order preserved with all merging
            var orderedFeatureDescriptors = AllFeaturesInOrder();

            // merge feature state into ordered list
            var orderedFeatureDescriptorsAndStates = orderedFeatureDescriptors
                .Select(featureDescriptor => new {
                    FeatureDescriptor = featureDescriptor,
                    FeatureState = shellState.Features.FirstOrDefault(s => s.Name == featureDescriptor.Name),
                })
                .Where(entry => entry.FeatureState != null);

            // get loaded feature information 
            var loadedFeatures = _extensionManager.LoadFeatures(orderedFeatureDescriptorsAndStates.Select(entry => entry.FeatureDescriptor)).ToArray();

            // merge loaded feature information into ordered list
            var loadedEntries = orderedFeatureDescriptorsAndStates.Select(
                entry => new {
                    Feature = loadedFeatures.SingleOrDefault(f => f.Descriptor == entry.FeatureDescriptor)
                              ?? new Feature {
                                  Descriptor = entry.FeatureDescriptor,
                                  ExportedTypes = Enumerable.Empty<Type>()
                              },
                    entry.FeatureDescriptor,
                    entry.FeatureState,
                });

            // find feature state that is beyond what's currently available from modules
            var additionalState = shellState.Features.Except(loadedEntries.Select(entry => entry.FeatureState));

            // create additional stub entries for the sake of firing state change events on missing features
            var allEntries = loadedEntries.Concat(additionalState.Select(featureState => {
                var featureDescriptor = new FeatureDescriptor {
                    Name = featureState.Name,
                    Extension = new ExtensionDescriptor {
                        Name = featureState.Name
                    }
                };
                return new {
                    Feature = new Feature {
                        Descriptor = featureDescriptor,
                        ExportedTypes = Enumerable.Empty<Type>(),
                    },
                    FeatureDescriptor = featureDescriptor,
                    FeatureState = featureState
                };
            }));

            // lower enabled states in reverse order
            foreach (var entry in allEntries.Reverse().Where(entry => entry.FeatureState.EnableState == ShellFeatureState.State.Falling)) {
                _featureEvents.Disable(entry.Feature);
                _stateManager.UpdateEnabledState(entry.FeatureState, ShellFeatureState.State.Down);
            }

            // lower installed states in reverse order
            foreach (var entry in allEntries.Reverse().Where(entry => entry.FeatureState.InstallState == ShellFeatureState.State.Falling)) {
                _featureEvents.Uninstall(entry.Feature);
                _stateManager.UpdateInstalledState(entry.FeatureState, ShellFeatureState.State.Down);
            }

            // raise install and enabled states in order
            foreach (var entry in allEntries.Where(entry => IsRising(entry.FeatureState))) {
                if (entry.FeatureState.InstallState == ShellFeatureState.State.Rising) {
                    _featureEvents.Install(entry.Feature);
                    _stateManager.UpdateInstalledState(entry.FeatureState, ShellFeatureState.State.Up);
                }
                if (entry.FeatureState.EnableState == ShellFeatureState.State.Rising) {
                    _featureEvents.Enable(entry.Feature);
                    _stateManager.UpdateEnabledState(entry.FeatureState, ShellFeatureState.State.Up);
                }
            }

            // re-fire if any event handlers initiated additional state changes
            FireApplyChangesIfNeeded();
        }

        private IEnumerable<FeatureDescriptor> AllFeaturesInOrder() {
            return OrderByDependencies(_extensionManager.AvailableExtensions().SelectMany(ext => ext.Features));
        }

        static bool IsRising(ShellFeatureState state) {
            return state.InstallState == ShellFeatureState.State.Rising ||
                   state.EnableState == ShellFeatureState.State.Rising;
        }

        class Linkage {
            public FeatureDescriptor Feature {
                get;
                set;
            }
            public bool Used {
                get;
                set;
            }
        }

        private static IEnumerable<FeatureDescriptor> OrderByDependencies(IEnumerable<FeatureDescriptor> descriptors) {
            var population = descriptors.Select(d => new Linkage {
                Feature = d
            }).ToArray();

            var result = new List<FeatureDescriptor>();
            foreach (var item in population) {
                Add(item, result, population);
            }
            return result;
        }

        private static void Add(Linkage item, ICollection<FeatureDescriptor> list, IEnumerable<Linkage> population) {
            if (item.Used)
                return;

            item.Used = true;
            var dependencies = item.Feature.Dependencies ?? Enumerable.Empty<string>();
            foreach (var dependency in dependencies.SelectMany(d => population.Where(p => p.Feature.Name == d))) {
                Add(dependency, list, population);
            }
            list.Add(item.Feature);
        }
    }
}

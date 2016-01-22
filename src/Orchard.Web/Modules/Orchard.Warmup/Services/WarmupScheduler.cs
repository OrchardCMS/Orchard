using System;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.State;
using Orchard.Events;

namespace Orchard.Warmup.Services {
    public interface IWarmupEventHandler : IEventHandler {
        void Generate(bool force);
    }

    public class WarmupScheduler : IWarmupScheduler, IWarmupEventHandler {
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly Lazy<IWarmupUpdater> _warmupUpdater;

        public WarmupScheduler(
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager,
            Lazy<IWarmupUpdater> warmupUpdater ) {
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;
            _warmupUpdater = warmupUpdater;
        }

        public void Schedule(bool force) {
            var shellDescriptor = _shellDescriptorManager.GetShellDescriptor();

            _processingEngine.AddTask(
                _shellSettings,
                shellDescriptor,
                "IWarmupEventHandler.Generate",
                new Dictionary<string, object> { { "force", force } }
                );
        }

        public void Generate(bool force) {
            if(force) {
                _warmupUpdater.Value.Generate();
            }
            else {
                _warmupUpdater.Value.EnsureGenerate();
            }
        }
    }
}
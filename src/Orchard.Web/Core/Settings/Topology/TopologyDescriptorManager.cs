using System;
using System.Collections.Generic;
using Orchard.Core.Settings.Topology.Records;
using Orchard.Data;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Events;

namespace Orchard.Core.Settings.Topology {
    public class TopologyDescriptorManager : ITopologyDescriptorManager {
        private readonly IRepository<TopologyRecord> _repository;
        private readonly IEventBus _eventBus;
        private int _serialNumber;

        public TopologyDescriptorManager(
            IRepository<TopologyRecord> repository,
            IEventBus eventBus) {
            _repository = repository;
            _eventBus = eventBus;
        }

        public ShellTopologyDescriptor GetTopologyDescriptor() {
            return _serialNumber == 0 ? null : new ShellTopologyDescriptor { SerialNumber = _serialNumber };
        }

        public void UpdateTopologyDescriptor(int priorSerialNumber, IEnumerable<TopologyFeature> enabledFeatures, IEnumerable<TopologyParameter> parameters) {
            if (priorSerialNumber != _serialNumber)
                throw new Exception();

            ++_serialNumber;
            _eventBus.Notify(
                typeof(ITopologyDescriptorManager).FullName + ".UpdateTopologyDescriptor",
                null);
        }
    }
}

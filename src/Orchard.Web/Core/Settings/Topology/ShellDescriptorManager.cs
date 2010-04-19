using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Core.Settings.Topology.Records;
using Orchard.Data;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Events;
using Orchard.Localization;

namespace Orchard.Core.Settings.Topology {
    public class TopologyDescriptorManager : ITopologyDescriptorManager {
        private readonly IRepository<TopologyRecord> _topologyRecordRepository;
        private readonly IEventBus _eventBus;

        public TopologyDescriptorManager(
            IRepository<TopologyRecord> repository,
            IEventBus eventBus) {
            _topologyRecordRepository = repository;
            _eventBus = eventBus;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        public ShellTopologyDescriptor GetTopologyDescriptor() {
            TopologyRecord topologyRecord = GetTopologyRecord();
            if (topologyRecord == null) return null;
            return GetShellTopologyDescriptorFromRecord(topologyRecord);
        }

        private static ShellTopologyDescriptor GetShellTopologyDescriptorFromRecord(TopologyRecord topologyRecord) {
            ShellTopologyDescriptor descriptor = new ShellTopologyDescriptor { SerialNumber = topologyRecord.SerialNumber };
            var descriptorFeatures = new List<TopologyFeature>();
            foreach (var topologyFeatureRecord in topologyRecord.EnabledFeatures) {
                descriptorFeatures.Add(new TopologyFeature { Name = topologyFeatureRecord.Name });
            }
            descriptor.EnabledFeatures = descriptorFeatures;
            var descriptorParameters = new List<TopologyParameter>();
            foreach (var topologyParameterRecord in topologyRecord.Parameters) {
                descriptorParameters.Add(
                    new TopologyParameter {
                        Component = topologyParameterRecord.Component,
                        Name = topologyParameterRecord.Name,
                        Value = topologyParameterRecord.Value
                    });
            }
            descriptor.Parameters = descriptorParameters;

            return descriptor;
        }

        private TopologyRecord GetTopologyRecord() {
            var records = from record in _topologyRecordRepository.Table select record;
            return records.FirstOrDefault();
        }

        public void UpdateTopologyDescriptor(int priorSerialNumber, IEnumerable<TopologyFeature> enabledFeatures, IEnumerable<TopologyParameter> parameters) {
            TopologyRecord topologyRecord = GetTopologyRecord();
            var serialNumber = topologyRecord == null ? 0 : topologyRecord.SerialNumber;
            if (priorSerialNumber != serialNumber)
                throw new InvalidOperationException(T("Invalid serial number for shell topology").ToString());

            if (topologyRecord == null) {
                serialNumber++;
                _topologyRecordRepository.Create(new TopologyRecord {
                    SerialNumber = serialNumber
                });
                topologyRecord = _topologyRecordRepository.Get(x => x.SerialNumber == serialNumber);
            }
            else {
                topologyRecord.SerialNumber++;
            }

            var descriptorFeatureRecords = new List<TopologyFeatureRecord>();
            foreach (var feature in enabledFeatures) {
                descriptorFeatureRecords.Add(new TopologyFeatureRecord { Name = feature.Name, TopologyRecord = topologyRecord});
            }
            topologyRecord.EnabledFeatures = descriptorFeatureRecords;

            var descriptorParameterRecords = new List<TopologyParameterRecord>();
            foreach (var parameter in parameters) {
                descriptorParameterRecords.Add(new TopologyParameterRecord {
                    Component = parameter.Component,
                    Name = parameter.Name,
                    Value = parameter.Value,
                    TopologyRecord = topologyRecord
                });
            }
            topologyRecord.EnabledFeatures = descriptorFeatureRecords;
            topologyRecord.Parameters = descriptorParameterRecords;

            _eventBus.Notify(
                typeof(ITopologyDescriptorManager).FullName + ".UpdateTopologyDescriptor",
                null);
        }
    }
}

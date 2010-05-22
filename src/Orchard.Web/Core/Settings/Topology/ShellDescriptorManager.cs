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
    public class ShellDescriptorManager : IShellDescriptorManager {
        private readonly IRepository<TopologyRecord> _topologyRecordRepository;
        private readonly IShellDescriptorManagerEventHandler _events;

        public ShellDescriptorManager(
            IRepository<TopologyRecord> repository,
            IShellDescriptorManagerEventHandler events) {
            _topologyRecordRepository = repository;
            _events = events;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        public ShellDescriptor GetShellDescriptor() {
            TopologyRecord topologyRecord = GetTopologyRecord();
            if (topologyRecord == null) return null;
            return GetShellTopologyDescriptorFromRecord(topologyRecord);
        }

        private static ShellDescriptor GetShellTopologyDescriptorFromRecord(TopologyRecord topologyRecord) {
            ShellDescriptor descriptor = new ShellDescriptor { SerialNumber = topologyRecord.SerialNumber };
            var descriptorFeatures = new List<ShellFeature>();
            foreach (var topologyFeatureRecord in topologyRecord.EnabledFeatures) {
                descriptorFeatures.Add(new ShellFeature { Name = topologyFeatureRecord.Name });
            }
            descriptor.EnabledFeatures = descriptorFeatures;
            var descriptorParameters = new List<ShellParameter>();
            foreach (var topologyParameterRecord in topologyRecord.Parameters) {
                descriptorParameters.Add(
                    new ShellParameter {
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

        public void UpdateShellDescriptor(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters) {
            TopologyRecord topologyRecord = GetTopologyRecord();
            var serialNumber = topologyRecord == null ? 0 : topologyRecord.SerialNumber;
            if (priorSerialNumber != serialNumber)
                throw new InvalidOperationException(T("Invalid serial number for shell topology").ToString());

            if (topologyRecord == null) {
                topologyRecord = new TopologyRecord {SerialNumber = 1};
                _topologyRecordRepository.Create(topologyRecord);
            }
            else {
                topologyRecord.SerialNumber++;
            }

            topologyRecord.EnabledFeatures.Clear();
            foreach (var feature in enabledFeatures) {
                topologyRecord.EnabledFeatures.Add(new TopologyFeatureRecord { Name = feature.Name, TopologyRecord = topologyRecord });
            }


            topologyRecord.Parameters.Clear();
            foreach (var parameter in parameters) {
                topologyRecord.Parameters.Add(new TopologyParameterRecord {
                    Component = parameter.Component,
                    Name = parameter.Name,
                    Value = parameter.Value,
                    TopologyRecord = topologyRecord
                });
            }

            _events.Changed(GetShellTopologyDescriptorFromRecord(topologyRecord));
        }
    }
}

using System;
using System.Collections.Generic;
using Orchard.Core.Settings.Topology.Records;
using Orchard.Data;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Localization;

namespace Orchard.Core.Settings.Topology {
    public class ShellDescriptorManager : IShellDescriptorManager {
        private readonly IRepository<ShellDescriptorRecord> _shellDescriptorRepository;
        private readonly IShellDescriptorManagerEventHandler _events;

        public ShellDescriptorManager(
            IRepository<ShellDescriptorRecord> shellDescriptorRepository,
            IShellDescriptorManagerEventHandler events) {
            _shellDescriptorRepository = shellDescriptorRepository;
            _events = events;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        public ShellDescriptor GetShellDescriptor() {
            ShellDescriptorRecord shellDescriptorRecord = GetTopologyRecord();
            if (shellDescriptorRecord == null) return null;
            return GetShellTopologyDescriptorFromRecord(shellDescriptorRecord);
        }

        private static ShellDescriptor GetShellTopologyDescriptorFromRecord(ShellDescriptorRecord shellDescriptorRecord) {
            ShellDescriptor descriptor = new ShellDescriptor { SerialNumber = shellDescriptorRecord.SerialNumber };
            var descriptorFeatures = new List<ShellFeature>();
            foreach (var topologyFeatureRecord in shellDescriptorRecord.Features) {
                descriptorFeatures.Add(new ShellFeature { Name = topologyFeatureRecord.Name });
            }
            descriptor.Features = descriptorFeatures;
            var descriptorParameters = new List<ShellParameter>();
            foreach (var topologyParameterRecord in shellDescriptorRecord.Parameters) {
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

        private ShellDescriptorRecord GetTopologyRecord() {
            return _shellDescriptorRepository.Get(x => true);
        }

        public void UpdateShellDescriptor(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters) {
            ShellDescriptorRecord shellDescriptorRecord = GetTopologyRecord();
            var serialNumber = shellDescriptorRecord == null ? 0 : shellDescriptorRecord.SerialNumber;
            if (priorSerialNumber != serialNumber)
                throw new InvalidOperationException(T("Invalid serial number for shell topology").ToString());

            if (shellDescriptorRecord == null) {
                shellDescriptorRecord = new ShellDescriptorRecord { SerialNumber = 1 };
                _shellDescriptorRepository.Create(shellDescriptorRecord);
            }
            else {
                shellDescriptorRecord.SerialNumber++;
            }

            shellDescriptorRecord.Features.Clear();
            foreach (var feature in enabledFeatures) {
                shellDescriptorRecord.Features.Add(new ShellFeatureRecord { Name = feature.Name, ShellDescriptorRecord = shellDescriptorRecord });
            }


            shellDescriptorRecord.Parameters.Clear();
            foreach (var parameter in parameters) {
                shellDescriptorRecord.Parameters.Add(new ShellParameterRecord {
                    Component = parameter.Component,
                    Name = parameter.Name,
                    Value = parameter.Value,
                    ShellDescriptorRecord = shellDescriptorRecord
                });
            }

            _events.Changed(GetShellTopologyDescriptorFromRecord(shellDescriptorRecord));
        }


    }
}

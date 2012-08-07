using System;
using System.Collections.Generic;
using Orchard.Core.Settings.Descriptor.Records;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;

namespace Orchard.Core.Settings.Descriptor {
    public class ShellDescriptorManager : IShellDescriptorManager {
        private readonly IRepository<ShellDescriptorRecord> _shellDescriptorRepository;
        private readonly IShellDescriptorManagerEventHandler _events;
        private readonly ShellSettings _shellSettings;

        public ShellDescriptorManager(
            IRepository<ShellDescriptorRecord> shellDescriptorRepository,
            IShellDescriptorManagerEventHandler events,
            ShellSettings shellSettings) {
            _shellDescriptorRepository = shellDescriptorRepository;
            _events = events;
            _shellSettings = shellSettings;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ShellDescriptor GetShellDescriptor() {
            ShellDescriptorRecord shellDescriptorRecord = GetDescriptorRecord();
            if (shellDescriptorRecord == null) return null;
            return GetShellDescriptorFromRecord(shellDescriptorRecord);
        }

        private static ShellDescriptor GetShellDescriptorFromRecord(ShellDescriptorRecord shellDescriptorRecord) {
            ShellDescriptor descriptor = new ShellDescriptor { SerialNumber = shellDescriptorRecord.SerialNumber };
            var descriptorFeatures = new List<ShellFeature>();
            foreach (var descriptorFeatureRecord in shellDescriptorRecord.Features) {
                descriptorFeatures.Add(new ShellFeature { Name = descriptorFeatureRecord.Name });
            }
            descriptor.Features = descriptorFeatures;
            var descriptorParameters = new List<ShellParameter>();
            foreach (var descriptorParameterRecord in shellDescriptorRecord.Parameters) {
                descriptorParameters.Add(
                    new ShellParameter {
                        Component = descriptorParameterRecord.Component,
                        Name = descriptorParameterRecord.Name,
                        Value = descriptorParameterRecord.Value
                    });
            }
            descriptor.Parameters = descriptorParameters;

            return descriptor;
        }

        private ShellDescriptorRecord GetDescriptorRecord() {
            return _shellDescriptorRepository.Get(x => x != null);
        }

        public void UpdateShellDescriptor(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters) {
            ShellDescriptorRecord shellDescriptorRecord = GetDescriptorRecord();
            var serialNumber = shellDescriptorRecord == null ? 0 : shellDescriptorRecord.SerialNumber;
            if (priorSerialNumber != serialNumber)
                throw new InvalidOperationException(T("Invalid serial number for shell descriptor").ToString());

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

            _events.Changed(GetShellDescriptorFromRecord(shellDescriptorRecord), _shellSettings.Name);
        }
    }
}

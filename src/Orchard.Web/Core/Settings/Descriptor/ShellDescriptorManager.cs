using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Core.Settings.Descriptor.Records;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Locking;
using Orchard.Logging;

namespace Orchard.Core.Settings.Descriptor {
    public class ShellDescriptorManager : Component, IShellDescriptorManager {
        private readonly IRepository<ShellDescriptorRecord> _shellDescriptorRepository;
        private readonly IShellDescriptorManagerEventHandler _events;
        private readonly ShellSettings _shellSettings;
        private readonly ILockingProvider _lockingProvider;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        
        public ShellDescriptorManager(
            IRepository<ShellDescriptorRecord> shellDescriptorRepository,
            IShellDescriptorManagerEventHandler events,
            ShellSettings shellSettings,
            ILockingProvider lockingProvider,
            ICacheManager cacheManager,
            ISignals signals) {

            _shellDescriptorRepository = shellDescriptorRepository;
            _events = events;
            _shellSettings = shellSettings;
            _lockingProvider = lockingProvider;
            _cacheManager = cacheManager;
            _signals = signals;

            _lockString = string.Join(".",
                _shellSettings["Name"] ?? "",
                "ShellDescriptorManager");
        }

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

        private const string EvictSignalName =
            "ShellDescriptorRecord_EvictCache";
        private const string DescriptorCacheName =
            "Orchard.Core.Settings.Descriptor.ShellDescriptorManager.ShellDescriptorRecord";
        private ShellDescriptorRecord GetDescriptorRecord() {
            // fetching the ShellDescriptorRecord also causes NHibernate to launch
            // SELECT queries to fetch the ShellFeatureRecords and the ShellParameterRecords.
            // If we cache all that, we save those three select queries on every request.
            // We should be careful in the eviction policy when the ShellDescriptorRecord
            // gets updated.
            return _cacheManager.Get(DescriptorCacheName, true, ctx => {
                ctx.Monitor(_signals.When(EvictSignalName));
                return _shellDescriptorRepository.Get(x => x != null);
            });
        }

        private string _lockString;

        public void UpdateShellDescriptor(
            int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters) {
            // This is where the shell descriptor will be updated.
            // Since we plan to cache it, this method will have to update the
            // actual records in the database, and then evict the cache.
            // We are going to put an application lock around this to prevent
            // issues when for some weird reason several updates are being attempted
            // concurrently.
            _lockingProvider.Lock(_lockString, () => {
                ShellDescriptorRecord shellDescriptorRecord = _shellDescriptorRepository.Get(x => x != null);
                var serialNumber = shellDescriptorRecord == null ? 0 : shellDescriptorRecord.SerialNumber;
                if (priorSerialNumber != serialNumber)
                    throw new InvalidOperationException(T("Invalid serial number for shell descriptor").ToString());

                Logger.Information("Updating shell descriptor for shell '{0}'...", _shellSettings.Name);

                if (shellDescriptorRecord == null) {
                    shellDescriptorRecord = new ShellDescriptorRecord { SerialNumber = 1 };
                    _shellDescriptorRepository.Create(shellDescriptorRecord);
                } else {
                    shellDescriptorRecord.SerialNumber++;
                }

                shellDescriptorRecord.Features.Clear();
                foreach (var feature in enabledFeatures) {
                    shellDescriptorRecord.Features.Add(new ShellFeatureRecord { Name = feature.Name, ShellDescriptorRecord = shellDescriptorRecord });
                }
                Logger.Debug("Enabled features for shell '{0}' set: {1}.", _shellSettings.Name, String.Join(", ", enabledFeatures.Select(feature => feature.Name)));


                shellDescriptorRecord.Parameters.Clear();
                foreach (var parameter in parameters) {
                    shellDescriptorRecord.Parameters.Add(new ShellParameterRecord {
                        Component = parameter.Component,
                        Name = parameter.Name,
                        Value = parameter.Value,
                        ShellDescriptorRecord = shellDescriptorRecord
                    });
                }

                _signals.Trigger(EvictSignalName);

                Logger.Debug("Parameters for shell '{0}' set: {1}.", _shellSettings.Name, String.Join(", ", parameters.Select(parameter => parameter.Name + "-" + parameter.Value)));

                Logger.Information("Shell descriptor updated for shell '{0}'.", _shellSettings.Name);

                _events.Changed(GetShellDescriptorFromRecord(GetDescriptorRecord()), _shellSettings.Name);
            });
        }
    }
}

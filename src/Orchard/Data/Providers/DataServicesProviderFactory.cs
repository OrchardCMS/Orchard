using System;
using System.Collections.Generic;
using Autofac.Features.Metadata;

namespace Orchard.Data.Providers {

    public delegate IDataServicesProvider CreateDataServicesProvider(string dataFolder, string connectionString);

    public class DataServicesProviderFactory : IDataServicesProviderFactory {
        private readonly IEnumerable<Meta<CreateDataServicesProvider>> _providers;

        public DataServicesProviderFactory(IEnumerable<Meta<CreateDataServicesProvider>> providers) {
            _providers = providers;
        }

        public IDataServicesProvider CreateProvider(DataServiceParameters parameters) {
            var factory = GetProviderFactory(parameters.Provider) ?? GetProviderFactory("SqlServer");

            return factory != null ? factory(parameters.DataFolder, parameters.ConnectionString) : null;
        }

        CreateDataServicesProvider GetProviderFactory(string providerName) {
            foreach (var providerMeta in _providers) {
                object name;
                if (!providerMeta.Metadata.TryGetValue("ProviderName", out name)) {
                    continue;
                }
                if (string.Equals(Convert.ToString(name), providerName, StringComparison.OrdinalIgnoreCase)) {
                    return providerMeta.Value;
                }
            }
            return null;
        }
    }
}

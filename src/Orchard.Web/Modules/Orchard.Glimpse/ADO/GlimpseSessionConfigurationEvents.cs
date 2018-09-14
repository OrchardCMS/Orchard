using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using NHibernate.Cfg;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Utility;

namespace Orchard.Glimpse.ADO {
    [OrchardFeature(FeatureNames.SQL)]
    public class GlimpseSessionConfigurationEvents : ISessionConfigurationEvents {
        public void Created(FluentConfiguration cfg, AutoPersistenceModel defaultModel) {}

        public void Prepared(FluentConfiguration cfg) {}

        public void Building(Configuration cfg) {}

        public void Finished(Configuration cfg) {
            cfg.SetProperty("connection.provider", "Orchard.Glimpse.ADO.GlimpseConnectionProvider, Orchard.Glimpse");
        }

        public void ComputingHash(Hash hash) {}
    }
}
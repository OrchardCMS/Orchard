using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using Orchard.Azure.MediaServices.Models.Records;
using NHibernate.Cfg;
using Orchard.Data;
using Orchard.Utility;

namespace Orchard.Azure.MediaServices.Infrastructure.Mappings {
    public class PersistenceConfiguration : ISessionConfigurationEvents {
        public void Created(FluentConfiguration cfg, AutoPersistenceModel defaultModel) {
            defaultModel.Override<AssetRecord>(mapping => mapping.IgnoreProperty(x => x.Infoset));
            defaultModel.Override<TaskRecord>(mapping => mapping.References(x => x.Job, "JobId"));
            defaultModel.Override<JobRecord>(mapping => mapping.HasMany(x => x.Tasks).KeyColumn("JobId"));
        }
        public void Prepared(FluentConfiguration cfg) {}
        public void Building(Configuration cfg) {}
        public void Finished(Configuration cfg) {}

        public void ComputingHash(Hash hash) {
            hash.AddString("AssetRecord.Ignore.InfoSet");
            hash.AddString("TaskRecord.References.Job.Via.JobId");
            hash.AddString("JobRecord.HasMany.Tasks.KeyColumn.JobId");
        }
    }
}
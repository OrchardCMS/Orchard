using FluentNHibernate.Mapping;

namespace Orchard.Data.Migration.Records {
    public class DataMigrationRecord {
        public virtual int Id { get; set; }
        public virtual string DataMigrationClass { get; set; }
        public virtual int Version { get; set; }
    }

    /// <summary>
    /// Since the "Version" colmuns is "AutoMapped" in FluentNHibernate, and the currently used version
    /// doesn't understand IVersionConvention and IVersionConventionAcceptance, we need to provide a manual
    /// mapping.
    /// </summary>
    public sealed class DataMigrationRecordMap : ClassMap<DataMigrationRecord> {
        public DataMigrationRecordMap() {
            Table("Orchard_Framework_DataMigrationRecord");
            Id(x => x.Id);
            Map(x => x.DataMigrationClass);
            Map(x => x.Version);
        }
    }
}
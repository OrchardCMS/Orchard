namespace Orchard.Data.Migration.Records {
    public class DataMigrationRecord {
        public virtual int Id { get; set; }
        public virtual string DataMigrationClass { get; set; }
        public virtual int? Version { get; set; }
    }
}
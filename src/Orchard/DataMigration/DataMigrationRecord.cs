namespace Orchard.DataMigration {
    public class DataMigrationRecord {
        public virtual int Id { get; set; }
        public virtual string DataMigrationClass { get; set; }
        public virtual int Current { get; set; }
    }
}
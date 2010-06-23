using Orchard.DataMigration.Schema;

namespace Orchard.DataMigration {
    /// <summary>
    /// Data Migration classes can inherit from this class to get a SchemaBuilder instance configured with the current tenant database prefix
    /// </summary>
    public abstract class DataMigrationImpl : IDataMigration {
        public abstract string Feature { get; }
        public SchemaBuilder SchemaBuilder { get; set; }
    }
}

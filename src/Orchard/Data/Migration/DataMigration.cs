using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Data.Migration {
    /// <summary>
    /// Data Migration classes can inherit from this class to get a SchemaBuilder instance configured with the current tenant database prefix
    /// </summary>
    public abstract class DataMigrationImpl : IDataMigration {
        public SchemaBuilder SchemaBuilder { get; set; }
        public IContentDefinitionManager ContentDefinitionManager {get; set; }
        public virtual Feature Feature { get; set; }
    }
}
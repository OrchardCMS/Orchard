namespace Orchard.DataMigration {
    public abstract class DataMigration : IDataMigration {
        public abstract string Feature { get; }
    }
}

namespace Orchard.DataMigration {
    public interface IDataMigrationManager : IDependency {
        void Upgrade(string feature);
    }
}
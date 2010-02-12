namespace Orchard.Data.Migrations {
    public interface IDatabaseMigrationManager : IDependency {
        IDatabaseCoordinator CreateCoordinator(string provider, string dataFolder, string connectionString);
    }
}

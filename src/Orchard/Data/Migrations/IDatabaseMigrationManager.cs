namespace Orchard.Data.Migrations {
    public interface IDatabaseMigrationManager {
        IDatabaseCoordinator CreateCoordinator(string provider, string dataFolder, string connectionString);
    }
}

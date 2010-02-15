namespace Orchard.Data.Migrations {

    public interface IDatabaseManager : IDependency {
        ISessionFactoryBuilder CreateCoordinator(DatabaseParameters databaseParameters);
    }

    public class DatabaseParameters {
        public string Provider { get; set; }
        public string DataFolder { get; set; }
        public string ConnectionString { get; set; }
    }
}

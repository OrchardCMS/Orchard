namespace Orchard.DataMigration {
    public interface IDataMigration : IDependency {
        string Feature { get; }
    }
}

namespace Orchard.Data.Providers {
    public interface ISqlStatementProvider : ISingletonDependency {
        string DataProvider { get; }
        string GetStatement(string command);
    }
}

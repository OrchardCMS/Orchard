using System;
using System.Data;
using Orchard;

namespace Upgrade.Services {
    public interface IUpgradeService : IDependency {
        void CopyTable(string fromTableName, string toTableName, string[] ignoreColumns);
        void ExecuteReader(string sqlStatement, Action<IDataReader, IDbConnection> action);
        string GetPrefixedTableName(string tableName);
        bool TableExists(string tableName);
    }
}

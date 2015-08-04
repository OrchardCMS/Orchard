using System.Collections.Generic;

namespace Orchard.ImportExport.Services {
    public interface IDatabaseManager : IDependency {
        IEnumerable<string> GetTenantDatabaseTableNames();
        void DropTenantDatabaseTables();
    }
}
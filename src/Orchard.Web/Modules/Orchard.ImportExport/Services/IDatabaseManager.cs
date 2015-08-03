using System.Collections.Generic;

namespace Orchard.ImportExport.Services {
    public interface IDatabaseManager {
        IEnumerable<string> GetTenantDatabaseTableNames();
        void DropTenantDatabaseTables();
    }
}
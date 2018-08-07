using System.Collections.Generic;
using Orchard.Data.Providers;

namespace Orchard.Indexing.Services {
    public class IndexingNoLockTableProvider : INoLockTableProvider {
        public IEnumerable<string> GetTableNames() {
            return new string[] { "Orchard_Indexing_IndexingTaskRecord" };
        }
    }
}
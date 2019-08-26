using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Data.Providers {
    public interface INoLockTableProvider : IDependency {
        /// <summary>
        /// Returns the names of the tables from which read operations should ignore shared locks.
        /// </summary>
        /// <returns>An IEnumerable<string> with the table names.</string></returns>
        /// <remarks>Implementations of this should not have to worry about table prefixes used to 
        /// discriminate between tenants sharing a db. That should be taken care of, if needed, where
        /// the results of this method are used. Implementations of this should avoid returning null.</remarks>
        IEnumerable<string> GetTableNames();
    }
}

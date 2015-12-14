using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Orchard.Core.Settings.State.Records {
    public class ShellStateRecord {
        public ShellStateRecord() {
            Features = new List<ShellFeatureStateRecord>();
        }

        public virtual int Id { get; set; }

        /// <summary>
        /// Workaround SqlCe: There is apparently no way to insert a row in a table with a single column IDENTITY ID primary key.
        /// See: http://www.sqldev.org/transactsql/insert-only-identity-column-value-in-sql-compact-edition-95267.shtml
        /// So we added this "Unused" column.
        ///  </summary>
        public virtual string Unused { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<ShellFeatureStateRecord> Features { get; set; }
    }
}
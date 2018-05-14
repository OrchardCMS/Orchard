using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.SqlCommand;
using Orchard.Data.Providers;
using Orchard.Environment.Configuration;

namespace Orchard.Data {
    public class NoLockInterceptor : EmptyInterceptor, ISessionInterceptor {

        private readonly ShellSettings _shellSettings;
        private readonly IEnumerable<INoLockTableProvider> _noLockTableProviders;

        public NoLockInterceptor(
            ShellSettings shellSettings,
            IEnumerable<INoLockTableProvider> noLockTableProviders) {

            _shellSettings = shellSettings;
            _noLockTableProviders = noLockTableProviders;
        }

        private List<string> _tableNames;
        public List<string> TableNames {
            get {
                if (_tableNames == null) {
                    _tableNames = new List<string>(
                        _noLockTableProviders
                            .SelectMany(nltp => nltp.GetTableNames())
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Select(n => GetPrefixedTableName(n.Trim())));
                }
                return _tableNames;
            }
        }

        private string GetPrefixedTableName(string tableName) {
            if (string.IsNullOrWhiteSpace(_shellSettings.DataTablePrefix)) {
                return tableName;
            }

            return _shellSettings.DataTablePrefix + "_" + tableName;
        }
        
        // based on https://stackoverflow.com/a/39518098/2669614
        public override SqlString OnPrepareStatement(SqlString sql) {

            // Modify the sql to add hints
            if (sql.StartsWithCaseInsensitive("select")) {
                var parts = sql.ToString().Split().ToList();
                var fromItem = parts.FirstOrDefault(p => p.Trim().Equals("from", StringComparison.OrdinalIgnoreCase));
                int fromIndex = fromItem != null ? parts.IndexOf(fromItem) : -1;
                var whereItem = parts.FirstOrDefault(p => p.Trim().Equals("where", StringComparison.OrdinalIgnoreCase));
                int whereIndex = whereItem != null ? parts.IndexOf(whereItem) : parts.Count;

                if (fromIndex == -1)
                    return sql;

                foreach (var tableName in TableNames) {
                    // set NOLOCK for each one of these tables
                    var tableItem = parts
                        .FirstOrDefault(p => p.Trim()
                            .Equals(tableName, StringComparison.OrdinalIgnoreCase));
                    if (tableItem != null) {
                        // the table is involved in this statement
                        var tableIndex = parts.IndexOf(tableItem);
                        // recompute whereIndex in case we added stuff to parts
                        whereIndex = whereItem != null ? parts.IndexOf(whereItem) : parts.Count;
                        if (tableIndex > fromIndex && tableIndex < whereIndex) { // sanity check
                            // if before the table name we have "," or "FROM", this is not a join, but rather
                            // something like "FROM tableName alias ..."
                            // we can insert "WITH (NOLOCK)" after that
                            if (tableIndex == fromIndex + 1
                                || parts[tableIndex - 1].Equals(",")) {

                                parts.Insert(tableIndex + 2, "WITH (NOLOCK)");
                            }
                            else {
                                // probably doing a join, so edit the next "on" and make it
                                // "WITH (NOLOCK) on"
                                for (int i = tableIndex + 1; i < whereIndex; i++) {
                                    if (parts[i].Trim().Equals("on", StringComparison.OrdinalIgnoreCase)) {
                                        parts[i] = "WITH (NOLOCK) on";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                // MUST use SqlString.Parse() method instead of new SqlString()
                sql = SqlString.Parse(string.Join(" ", parts));
            }

            return sql;
        }
    }
}

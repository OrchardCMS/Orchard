using System.Collections.Generic;

namespace Orchard.DataMigration.Schema {
    public class SqlStatementCommand : SchemaCommand {
        protected readonly List<string> _dialects;
        public SqlStatementCommand(string sql)
            : base("") {
            Sql = sql;
            _dialects = new List<string>();
        }

        public string Sql { get; private set; }

        public SqlStatementCommand ForDialect(string dialect) {
            _dialects.Add(dialect);
            return this;
        }
    }
}

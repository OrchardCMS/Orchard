using System.Collections.Generic;

namespace Orchard.DataMigration.Schema {
    public class SchemaCommand {
        protected readonly List<TableCommand> _tableCommands;

        public SchemaCommand(string tableName) {
            _tableCommands = new List<TableCommand>();
            Name(tableName);
        }

        public string TableName { get; private set; }

        public SchemaCommand Name(string name) {
            TableName = name;
            return this;
        }
    }
}

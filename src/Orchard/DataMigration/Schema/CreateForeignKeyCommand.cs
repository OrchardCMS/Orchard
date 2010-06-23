using System.Collections.Generic;

namespace Orchard.DataMigration.Schema {
    public class CreateForeignKeyCommand : TableCommand {
        protected readonly List<ForeignKeyClause> _foreignKeyClauses;

        public CreateForeignKeyCommand(string name)
            : base(name) {
                _foreignKeyClauses = new List<ForeignKeyClause>();
        }

        public CreateForeignKeyCommand On(string srcColumn, string destTable, string destColumn) {
            _foreignKeyClauses.Add(new ForeignKeyClause(srcColumn, destTable, destColumn));
            return this;
        }
    }

    public class ForeignKeyClause {
        public ForeignKeyClause(string srcColumn, string destTable, string destColumn) {
            SrcColumn = srcColumn;
            DestTable = destTable;
            DestColumn = destColumn;
        }

        public string DestColumn { get; private set; }

        public string DestTable { get; private set; }

        public string SrcColumn { get; private set; }
    }
}

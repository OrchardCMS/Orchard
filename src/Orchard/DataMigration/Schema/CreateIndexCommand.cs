namespace Orchard.DataMigration.Schema {
    public class CreateIndexCommand : TableCommand {
        public CreateIndexCommand(string name, params string[] columnNames)
            : base(name) {
            ColumnNames = columnNames;
        }

        public string[] ColumnNames { get; private set; }
    }
}

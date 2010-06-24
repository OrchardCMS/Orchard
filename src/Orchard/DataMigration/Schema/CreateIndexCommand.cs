namespace Orchard.DataMigration.Schema {
    public class CreateIndexCommand : TableCommand {
        public CreateIndexCommand(string indexName, params string[] columnNames)
            : base(indexName) {
            ColumnNames = columnNames;
        }

        public string[] ColumnNames { get; private set; }
    }
}

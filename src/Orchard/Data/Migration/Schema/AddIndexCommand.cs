namespace Orchard.Data.Migration.Schema {
    public class AddIndexCommand : TableCommand {
        public string IndexName { get; set; }

        public AddIndexCommand(string tableName, string indexName, params string[] columnNames)
            : base(tableName) {
            ColumnNames = columnNames;
            IndexName = indexName;
        }

        public string[] ColumnNames { get; private set; }
    }
}

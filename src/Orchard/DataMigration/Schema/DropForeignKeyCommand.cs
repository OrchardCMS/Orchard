namespace Orchard.DataMigration.Schema {
    public class DropForeignKeyCommand : SchemaCommand {
        public string SrcTable { get; private set; }

        public DropForeignKeyCommand(string srcTable, string name)
            : base(name, SchemaCommandType.DropForeignKey) {
            SrcTable = srcTable;
        }
    }
}

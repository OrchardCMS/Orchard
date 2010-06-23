namespace Orchard.DataMigration.Schema {
    public class DropForeignKeyCommand : TableCommand {

        public DropForeignKeyCommand(string name)
            : base(name) {
        }
    }
}

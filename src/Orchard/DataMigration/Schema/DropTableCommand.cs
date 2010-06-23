namespace Orchard.DataMigration.Schema {
    public class DropTableCommand : SchemaCommand {
        public DropTableCommand(string name)
            : base(name) {
        }
    }
}

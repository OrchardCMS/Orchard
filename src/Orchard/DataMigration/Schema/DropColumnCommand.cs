namespace Orchard.DataMigration.Schema {
    public class DropColumnCommand : ColumnCommand {
        public DropColumnCommand(string name)
            : base(name) {
            
        }
    }
}

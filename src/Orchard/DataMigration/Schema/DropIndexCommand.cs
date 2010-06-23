namespace Orchard.DataMigration.Schema {
    public class DropIndexCommand : TableCommand {

        public DropIndexCommand(string name)
            : base(name) {
        }
    }
}

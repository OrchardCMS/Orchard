namespace Orchard.DataMigration.Schema {
    public class AlterColumnCommand : ColumnCommand {
        private string _newName;

        public AlterColumnCommand(string name)
            : base(name) {
        }


        public AlterColumnCommand Rename(string name) {
            _newName = name;
            return this;
        }
    }
}

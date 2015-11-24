namespace Orchard.Data.Migration.Schema {
    public class DropUniqueConstraintCommand : TableCommand {
        public string ConstraintName { get; set; }

        public DropUniqueConstraintCommand(string tableName, string constraintName)
            : base(tableName) {
            ConstraintName = constraintName;
        }
    }
}

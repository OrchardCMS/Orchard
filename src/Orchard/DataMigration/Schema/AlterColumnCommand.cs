using System.Data;

namespace Orchard.DataMigration.Schema {
    public class AlterColumnCommand : ColumnCommand {
        public AlterColumnCommand(string tableName, string columnName)
            : base(tableName, columnName) {
        }

        public new AlterColumnCommand WithType(DbType dbType) {
            base.WithType(dbType);
            return this;
        }

        public AlterColumnCommand WithType(DbType dbType, int? length) {
            base.WithType(dbType).WithLength(length);
            return this;
        }

        public AlterColumnCommand WithType(DbType dbType, byte precision, byte scale) {
            base.WithType(dbType);
            Precision = precision;
            Scale = scale;
            return this;
        }

    }
}

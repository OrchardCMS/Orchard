using System.Data;

namespace Orchard.Data.Migration.Schema {
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

        public new AlterColumnCommand WithLength(int? length) {
            base.WithLength(length);
            return this;
        }
        
        public new AlterColumnCommand Unlimited() {
            return WithLength(10000);
        }

    }
}

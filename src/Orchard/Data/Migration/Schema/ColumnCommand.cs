using System.Data;

namespace Orchard.Data.Migration.Schema {
    public class ColumnCommand : TableCommand {
        public string ColumnName { get; set; }

        public ColumnCommand(string tableName, string name)
            : base(tableName) {
            ColumnName = name;
            DbType = DbType.Object;
            Default = null;
            Length = null;
        }
        public byte Scale { get; protected set; }

        public byte Precision { get; protected set; }

        public DbType DbType { get; private set; }

        public object Default { get; private set; }

        public int? Length { get; private set; }

        public ColumnCommand WithType(DbType dbType) {
            DbType = dbType;
            return this;
        }

        public ColumnCommand WithDefault(object @default) {
            Default = @default;
            return this;
        }


        public ColumnCommand WithLength(int? length) {
            Length = length;
            return this;
        }

        public ColumnCommand Unlimited() {
            return WithLength(10000);
        }
    }
}

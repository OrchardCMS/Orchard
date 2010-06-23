using System.Data;

namespace Orchard.DataMigration.Schema {
    public class ColumnCommand : TableCommand {
        private DbType _dbType;
        private object _default;
        public ColumnCommand(string name) : base(name) {
            _dbType = DbType.Object;
            _default = null;
        }

        public ColumnCommand Type(DbType dbType) {
            _dbType = dbType;
            return this;
        }

        public ColumnCommand Default(object @default) {
            _default = @default;
            return this;
        }

   }
}

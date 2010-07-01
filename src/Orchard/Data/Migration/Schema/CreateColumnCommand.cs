using System.Data;

namespace Orchard.Data.Migration.Schema {
    public class CreateColumnCommand : ColumnCommand {
        public CreateColumnCommand(string tableName, string name) : base(tableName, name) {
            IsNotNull = false;
            IsUnique = false;
        }

        public bool IsUnique { get; protected set; }

        public bool IsNotNull { get; protected set; }

        public bool IsPrimaryKey { get; protected set; }

        public CreateColumnCommand PrimaryKey() {
            IsPrimaryKey = true;
            IsUnique = false;
            return this;
        }

        public CreateColumnCommand WithPrecision(byte precision) {
            Precision = precision;
            return this;
        }

        public CreateColumnCommand WithScale(byte scale) {
            Scale = scale;
            return this;
        }

        public CreateColumnCommand NotNull() {
            IsNotNull = true;
            return this;
        }

        public CreateColumnCommand Nullable() {
            IsNotNull = false;
            return this;
        }

        public CreateColumnCommand Unique() {
            IsUnique = true;
            IsPrimaryKey = false;
            return this;
        }

        public CreateColumnCommand NotUnique() {
            IsUnique = false;
            return this;
        }

        public new CreateColumnCommand WithLength(int? length) {
            base.WithLength(length);
            return this;
        }

        public new CreateColumnCommand WithType(DbType dbType) {
            base.WithType(dbType);
            return this;
        }

        public new CreateColumnCommand WithDefault(string @default) {
            base.WithDefault(@default);
            return this;
        }
    }
}

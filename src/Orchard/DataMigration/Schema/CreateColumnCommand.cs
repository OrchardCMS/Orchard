namespace Orchard.DataMigration.Schema {
    public class CreateColumnCommand : ColumnCommand {
        private bool _primaryKey;
        private byte? _precision;
        private byte? _scale;
        private int? _length;
        private bool _notNull;
        private bool _unique;
        

        public CreateColumnCommand(string name) : base(name) {
            _precision = null;
            _scale = null;
            _length = null;
            _notNull = false;
            _unique = false;
            
        }

        public CreateColumnCommand PrimaryKey() {
            _primaryKey = true;
            return this;
        }

        public CreateColumnCommand Precision(byte? precision) {
            _precision = precision;
            return this;
        }

        public CreateColumnCommand Scale(byte? scale) {
            _scale = scale;
            return this;
        }

        public CreateColumnCommand Length(int? length) {
            _length = length;
            return this;
        }

        public CreateColumnCommand NotNull() {
            _notNull = true;
            return this;
        }

        public CreateColumnCommand Nullable() {
            _notNull = false;
            return this;
        }

        public CreateColumnCommand Unique() {
            _unique = true;
            return this;
        }

        public CreateColumnCommand NotUnique() {
            _unique = false;
            return this;
        }
    }
}

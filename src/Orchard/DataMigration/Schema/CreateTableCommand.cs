using System;
using System.Data;

namespace Orchard.DataMigration.Schema {
    public class CreateTableCommand : SchemaCommand {
        public CreateTableCommand(string name)
            : base(name) {
        }

        public CreateTableCommand Column(string name, DbType dbType, Action<CreateColumnCommand> column = null) {
            var command = new CreateColumnCommand(name);
            command.Type(dbType);

            if ( column != null ) {
                column(command);
            }
            _tableCommands.Add(command);
            return this;
        }

        public CreateTableCommand ContentPartRecord() {
            /// TODO: Call Column() with necessary information for content part records
            return this;
        }

        public CreateTableCommand VersionedContentPartRecord() {
            /// TODO: Call Column() with necessary information for content part records
            return this;
        }

        public CreateTableCommand ForeignKey(string name, Action<CreateForeignKeyCommand> fk) {
            var command = new CreateForeignKeyCommand(name);
            fk(command);
            _tableCommands.Add(command);
            return this;
        }
    }
}

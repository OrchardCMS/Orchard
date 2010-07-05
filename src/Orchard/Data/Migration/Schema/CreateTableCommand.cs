using System;
using System.Data;

namespace Orchard.Data.Migration.Schema {
    public class CreateTableCommand : SchemaCommand {
        public CreateTableCommand(string name)
            : base(name, SchemaCommandType.CreateTable) {
        }

        public CreateTableCommand Column(string columnName, DbType dbType, Action<CreateColumnCommand> column = null) {
            var command = new CreateColumnCommand(Name, columnName);
            command.WithType(dbType);

            if ( column != null ) {
                column(command);
            }
            TableCommands.Add(command);
            return this;
        }

        public CreateTableCommand Column<T>(string columnName, Action<CreateColumnCommand> column = null) {
            var dbType = SchemaUtils.ToDbType(typeof (T));
            return Column(columnName, dbType, column);
        }

        public CreateTableCommand ContentPartRecord() {
            /// TODO: Call Column() with necessary information for content part records
            return this;
        }

        public CreateTableCommand VersionedContentPartRecord() {
            /// TODO: Call Column() with necessary information for content part records
            return this;
        }

        
    }
}

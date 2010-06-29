using System;
using System.Data;

namespace Orchard.DataMigration.Schema {
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
            var dbType = System.Data.DbType.Object;
            switch(System.Type.GetTypeCode(typeof(T))) {
                case TypeCode.String :
                    dbType = DbType.String;
                    break;
                case TypeCode.Int32 :
                    dbType = DbType.Int32;
                    break;
                default:
                    Enum.TryParse(System.Type.GetTypeCode(typeof (T)).ToString(), true, out dbType);
                    break;
            }

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

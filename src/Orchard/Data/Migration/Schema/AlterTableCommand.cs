using System;
using System.Data;

namespace Orchard.Data.Migration.Schema {
    public class AlterTableCommand : SchemaCommand {
        public AlterTableCommand(string name)
            : base(name, SchemaCommandType.AlterTable) {
        }

        public void AddColumn(string columnName, DbType dbType, Action<AddColumnCommand> column = null) {
            var command = new AddColumnCommand(Name, columnName);
            command.WithType(dbType);
            
            if(column != null) {
                column(command);
            }
            
            TableCommands.Add(command);
        }

        public void AddColumn<T>(string columnName, Action<AddColumnCommand> column = null) {
            var dbType = SchemaUtils.ToDbType(typeof(T));
            AddColumn(columnName, dbType, column);
        }

        public void DropColumn(string columnName) {
            var command = new DropColumnCommand(Name, columnName);
            TableCommands.Add(command);
        }

        public void AlterColumn(string columnName, Action<AlterColumnCommand> column = null) {
            var command = new AlterColumnCommand(Name, columnName);

            if ( column != null ) {
                column(command);
            }

            TableCommands.Add(command);
        }

        public void CreateIndex(string indexName, params string[] columnNames) {
            var command = new AddIndexCommand(Name, indexName, columnNames);
            TableCommands.Add(command);
        }

        public void DropIndex(string indexName) {
            var command = new DropIndexCommand(Name, indexName);
            TableCommands.Add(command);
        }
    }
}

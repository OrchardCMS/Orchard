using System;
using System.Data;
using JetBrains.Annotations;

namespace Orchard.DataMigration.Schema {
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

        public void CreateIndex(string name, params string[] columnNames) {
            var command = new CreateIndexCommand(name, columnNames);
            TableCommands.Add(command);
        }

        public void DropIndex(string name) {
            var command = new DropIndexCommand(name);
            TableCommands.Add(command);
        }
    }
}

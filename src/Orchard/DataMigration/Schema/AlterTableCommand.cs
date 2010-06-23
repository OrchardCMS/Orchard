using System;
using System.Data;

namespace Orchard.DataMigration.Schema {
    public class AlterTableCommand : SchemaCommand {
        public AlterTableCommand(string name)
            : base(name) {
        }

        public AlterTableCommand AddColumn(string name, DbType dbType, Action<CreateColumnCommand> column = null) {
            var command = new CreateColumnCommand(name);
            command.Type(dbType);
            
            if(column != null) {
                column(command);
            }
            
            _tableCommands.Add(command);
            return this;
        }

        public AlterTableCommand DropColumn(string name) {
            var command = new DropColumnCommand(name);
            _tableCommands.Add(command);
            return this;
        }

        public AlterTableCommand AlterColumn(string name, Action<AlterColumnCommand> column = null) {
            var command = new AlterColumnCommand(name);

            if ( column != null ) {
                column(command);
            }

            _tableCommands.Add(command);
            return this;
        }

        public AlterTableCommand CreateIndex(string name, params string[] columnNames) {
            var command = new CreateIndexCommand(name, columnNames);
            _tableCommands.Add(command);
            return this;
        }

        public AlterTableCommand DropIndex(string name) {
            var command = new DropIndexCommand(name);
            _tableCommands.Add(command);
            return this;
        }

        public AlterTableCommand AddForeignKey(string name, Action<CreateForeignKeyCommand> fk) {
            var command = new CreateForeignKeyCommand(name);
            fk(command);
            _tableCommands.Add(command);
            return this;
        }

        public AlterTableCommand DropForeignKey(string name) {
            var command = new DropForeignKeyCommand(name);
            _tableCommands.Add(command);
            return this;
        }
    }
}

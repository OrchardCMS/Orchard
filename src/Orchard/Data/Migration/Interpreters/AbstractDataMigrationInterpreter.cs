using Orchard.Data.Migration.Schema;

namespace Orchard.Data.Migration.Interpreters {
    public abstract class AbstractDataMigrationInterpreter {

        public void Visit(ISchemaBuilderCommand command) {
            var schemaCommand = command as SchemaCommand;
            if (schemaCommand == null) {
                return;
            }

            switch ( schemaCommand.Type ) {
                case SchemaCommandType.CreateTable:
                    Visit((CreateTableCommand)schemaCommand);
                    break;
                case SchemaCommandType.AlterTable:
                    Visit((AlterTableCommand)schemaCommand);
                    break;
                case SchemaCommandType.DropTable:
                    Visit((DropTableCommand)schemaCommand);
                    break;
                case SchemaCommandType.SqlStatement:
                    Visit((SqlStatementCommand)schemaCommand);
                    break;
                case SchemaCommandType.CreateForeignKey:
                    Visit((CreateForeignKeyCommand)schemaCommand);
                    break;
                case SchemaCommandType.DropForeignKey:
                    Visit((DropForeignKeyCommand)schemaCommand);
                    break;
            }
        }

        public abstract void Visit(CreateTableCommand command);
        public abstract void Visit(AlterTableCommand command);
        public abstract void Visit(DropTableCommand command);
        public abstract void Visit(SqlStatementCommand command);
        public abstract void Visit(CreateForeignKeyCommand command);
        public abstract void Visit(DropForeignKeyCommand command);
    }
}

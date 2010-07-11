using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;

public class NullInterpreter : IDataMigrationInterpreter {

    public void Visit(ISchemaBuilderCommand command) {
    }

    public void Visit(CreateTableCommand command) {
    }

    public void Visit(DropTableCommand command) {
    }

    public void Visit(AlterTableCommand command) {
    }

    public void Visit(SqlStatementCommand command) {
    }

    public void Visit(CreateForeignKeyCommand command) {
    }

    public void Visit(DropForeignKeyCommand command) {
    }
}
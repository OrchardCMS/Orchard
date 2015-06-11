using Orchard.Data.Migration.Schema;

namespace Orchard.Data.Migration.Interpreters {
    public class SQLiteCommandInterpreter :
        ICommandInterpreter<DropColumnCommand>,
        ICommandInterpreter<AlterColumnCommand>,
        ICommandInterpreter<CreateForeignKeyCommand>,
        ICommandInterpreter<DropForeignKeyCommand>,
        ICommandInterpreter<AddIndexCommand>,
        ICommandInterpreter<DropIndexCommand> {

        public string[] CreateStatements(DropColumnCommand command) {
            return new string[0];
        }

        public string[] CreateStatements(AlterColumnCommand command) {
            return new string[0];
        }

        public string[] CreateStatements(CreateForeignKeyCommand command) {
            return new string[0];
        }

        public string[] CreateStatements(DropForeignKeyCommand command) {
            return new string[0];
        }

        public string[] CreateStatements(AddIndexCommand command) {
            return new string[0];
        }

        public string[] CreateStatements(DropIndexCommand command) {
            return new string[0];
        }

        public string DataProvider {
            get { return "SQLite"; }
        }
    }
}

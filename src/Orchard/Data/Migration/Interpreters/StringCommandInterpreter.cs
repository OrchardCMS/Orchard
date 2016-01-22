using System.IO;
using Orchard.Data.Migration.Schema;

namespace Orchard.Data.Migration.Interpreters {
    public class StringCommandInterpreter : AbstractDataMigrationInterpreter {
        private readonly TextWriter _output;

        public StringCommandInterpreter(TextWriter output) {
            _output = output;
        }

        public override void Visit(CreateTableCommand command) {
            _output.WriteLine("Creating table {0}", command.Name);
        }

        public override void Visit(AlterTableCommand command) {
            _output.WriteLine("Altering table {0}", command.Name);
        }

        public override void Visit(DropTableCommand command) {
            _output.WriteLine("Dropping table {0}", command.Name);
        }

        public override void Visit(SqlStatementCommand command) {
            _output.WriteLine("Executing sql statement\n\n {0}", command.Sql);
        }

        public override void Visit(CreateForeignKeyCommand command) {
            _output.WriteLine("Creating foreign key {0}", command.Name);
        }

        public override void Visit(DropForeignKeyCommand command) {
            _output.WriteLine("Dropping foreign key {0}", command.Name);
        }
    }
}

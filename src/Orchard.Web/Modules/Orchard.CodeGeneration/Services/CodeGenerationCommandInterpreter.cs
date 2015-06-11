using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;

namespace Orchard.CodeGeneration.Services {
    public class CodeGenerationCommandInterpreter : AbstractDataMigrationInterpreter {
        private readonly TextWriter _output;

        public CodeGenerationCommandInterpreter(TextWriter output) {
            _output = output;
        }

        public override void Visit(CreateTableCommand command) {
            _output.WriteLine("\t\t\t// Creating table {0}", command.Name);
            _output.WriteLine("\t\t\tSchemaBuilder.CreateTable(\"{0}\", table => table", command.Name);

            var matchContentPartRecord = command.TableCommands.OfType<CreateColumnCommand>().Any(
                c =>
                c.IsPrimaryKey
                && c.ColumnName == "Id"
                && !c.IsIdentity
                && c.DbType == DbType.Int32);

            var matchContentPartVersionRecord = matchContentPartRecord && command.TableCommands.OfType<CreateColumnCommand>().Any(
                c =>
                c.ColumnName == "ContentItemRecord_id"
                && c.DbType == DbType.Int32);

            if ( matchContentPartVersionRecord ) {
                _output.WriteLine("\t\t\t\t.ContentPartVersionRecord()");
            }
            else if ( matchContentPartRecord ) {
                _output.WriteLine("\t\t\t\t.ContentPartRecord()");
            }

            foreach ( var createColumn in command.TableCommands.OfType<CreateColumnCommand>() ) {
                if(createColumn.ColumnName == "Id" && matchContentPartRecord) {
                    continue;
                }

                if(createColumn.ColumnName == "ContentItemRecord_id" && matchContentPartVersionRecord) {
                    continue;
                }

                var type = createColumn.DbType.ToString();
                var field = createColumn.ColumnName;
                var options = new List<string>();

                if ( createColumn.IsPrimaryKey ) {
                    options.Add("PrimaryKey()");
                }

                if ( createColumn.IsIdentity ) {
                    options.Add("Identity()");
                }

                if ( createColumn.IsUnique ) {
                    options.Add("Unique()");
                }

                if ( createColumn.IsNotNull ) {
                    options.Add("NotNull()");
                }

                if ( createColumn.Length.HasValue ) {
                    if ( createColumn.Length == 10000 ) {
                        options.Add("Unlimited()");
                    }
                    else {
                        options.Add(string.Format("WithLength({0})", createColumn.Length));
                    }
                }

                if ( createColumn.Precision > 0 ) {
                    options.Add(string.Format("WithPrecision({0})", createColumn.Precision));
                    options.Add(string.Format("WithScale({0})", createColumn.Scale));
                }

                _output.WriteLine("\t\t\t\t.Column(\"{0}\", DbType.{1}{2})", field, type, options.Any() ? ", column => column." + string.Join(".", options) : string.Empty);
            }

            _output.WriteLine("\t\t\t);");

        }

        public override void Visit(AlterTableCommand command) {
            _output.WriteLine("// Altering table {0}", command.Name);
        }

        public override void Visit(DropTableCommand command) {
            _output.WriteLine("// Dropping table {0}", command.Name);
            _output.WriteLine("\t\t\tSchemaBuilder.DropTable(\"{0}\", command.Name);");
        }

        public override void Visit(SqlStatementCommand command) {
            _output.WriteLine("// Executing sql statement\n\n {0}", command.Sql);
        }

        public override void Visit(CreateForeignKeyCommand command) {
            _output.WriteLine("// Creating foreign key {0}", command.Name);
        }

        public override void Visit(DropForeignKeyCommand command) {
            _output.WriteLine("// Dropping foreign key {0}", command.Name);
        }

    }
}

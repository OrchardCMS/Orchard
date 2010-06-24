using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.DataMigration.Schema;

namespace Orchard.DataMigration.Interpreters {
    public class SqLiteCommandInterpreter : 
        ICommandInterpreter<DropColumnCommand>,
        ICommandInterpreter<AlterColumnCommand>,        
        ICommandInterpreter<CreateForeignKeyCommand>,
        ICommandInterpreter<DropForeignKeyCommand> {
        
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

        public string DataProvider {
            get { return "SQLite"; }
        }
        }
}

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Orchard.Data.Migration.Schema {
    public abstract class SchemaCommand : ISchemaBuilderCommand {
        protected SchemaCommand(string name, SchemaCommandType type ) {
            TableCommands = new List<TableCommand>();
            Type = type;
            WithName(name);
        }

        public string Name { get; private set; }
        public List<TableCommand> TableCommands { get; private set; }

        public SchemaCommandType Type { get; [UsedImplicitly]private set; }

        public SchemaCommand WithName(string name) {
            Name = name;
            return this;
        }
    }

    public enum SchemaCommandType {
        CreateTable,
        DropTable,
        AlterTable,
        SqlStatement,
        CreateForeignKey,
        DropForeignKey
    }
}

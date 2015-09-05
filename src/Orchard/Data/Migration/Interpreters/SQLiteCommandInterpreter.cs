using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using NHibernate;
using NHibernate.Dialect;
using NHibernate.SqlTypes;
using Orchard.ContentManagement.Records;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;
using Outercurve.SQLiteCreateTree;
using Outercurve.SQLiteCreateTree.AlterTable;
using Outercurve.SQLiteCreateTree.Nodes;
using Outercurve.SQLiteCreateTree.Nodes.ColumnConstraint;
using OcfAction = Outercurve.SQLiteCreateTree.AlterTable.Action;

namespace Orchard.Data.Migration.Interpreters {
    public class SQLiteCommandInterpreter : ICommandInterpreter<CreateTableCommand>,
        ICommandInterpreter<AlterTableCommand>,
        //ICommandInterpreter<CreateForeignKeyCommand>,
        ICommandInterpreter<DropForeignKeyCommand> {


        //private readonly ISession _session;
        private readonly Dialect _dialect;
        private readonly ShellSettings _shellSettings;
        //private readonly DefaultDataMigrationInterpreter _dataMigrationInterpreter;

        //public SQLiteCommandInterpreter(DefaultDataMigrationInterpreter dataMigrationInterpreter) {
        //    _dataMigrationInterpreter = dataMigrationInterpreter;
        //}

        public SQLiteCommandInterpreter(
            ISessionLocator locator,
            ShellSettings shellSettings, 
            //ITransactionManager transactionManager,
            ISessionFactoryHolder sessionFactoryHolder,
            IDialectService dialectService
            ) {
            //_session = transactionManager.GetSession();
            _shellSettings = shellSettings;

            var configuration = sessionFactoryHolder.GetConfiguration();

            _dialect = dialectService.GetDialect(configuration);
        }

        public string DataProvider {
            get { return "SQLite"; }
        }

        public string[] CreateStatements(CreateTableCommand command) {
            var createTable = new CreateTableNode {
                TableName = PrefixTableName(command.Name), ColumnDefinitions = command.TableCommands.OfType<CreateColumnCommand>().
                    Select(CreateColumnDefNode)
                    .ToList()
            };
            var visitor = new TreeStringOutputVisitor();

            string[] ret = createTable.Accept(visitor).ToString().Split(new[] {System.Environment.NewLine}, StringSplitOptions.None);

            return ret;
        }

        public string[] CreateStatements(DropForeignKeyCommand command) {
            return new string[0];
        }

        public string[] CreateStatements(AlterTableCommand command) {
            return new string[0];
        }

        //private IEnumerable<string> GetIndexNodesForTable(string tableName) {
        //    var query = _session.CreateSQLQuery(String.Format("SELECT sql FROM sqlite_master WHERE tbl_name = '{0}' AND type = 'index'", tableName));
        //    var indexStrings = query.List<string>();
        //    return indexStrings;
        //}

        //private string GetCreateTableNodeFromSession(string tableName) {
        //    var query = _session.CreateSQLQuery(String.Format("SELECT sql FROM sqlite_master WHERE tbl_name = '{0}' AND type = 'table'", tableName));
        //    var createString = query.UniqueResult<string>();
        //    if (createString == null) {
        //        // we hav ea problem
        //        return null;
        //    }

        //    return createString;
        //}

        private string PrefixTableName(string tableName) {
            return PrefixTableName(_shellSettings, tableName);
        }
        public static string PrefixTableName(ShellSettings shellSettings, string tableName)
        {
            if (string.IsNullOrEmpty(shellSettings.DataTablePrefix))
            {
                return tableName;
            }
            return shellSettings.DataTablePrefix + "_" + tableName;
        }

        private OrchardToSQLiteAdapter CreateOrchardToSQLiteAdapter() {
            return new OrchardToSQLiteAdapter(_shellSettings, _dialect); 
        }

        private  ColumnDefNode CreateColumnDefNode(CreateColumnCommand command)
        {
            var ret = new ColumnDefNode { ColumnName = command.ColumnName, ColumnConstraints = new List<ColumnConstraintNode>() };

            //dialect converts DbType.Int16-64 to "INT" not "INTEGER" and only INTEGER columns can be autoincremented. This fixes that.
            string correctType = command.IsIdentity ? "INTEGER" : _dialect.GetTypeName(new SqlType(command.DbType));

            ret.TypeNameNode = SQLiteParseVisitor.ParseString<TypeNameNode>(correctType, i => i.type_name());

            //not quite right but should work

            if (command.IsIdentity || command.IsPrimaryKey)
            {
                var primKey = new PrimaryKeyConstraintNode();
                if (command.IsIdentity)
                {
                    primKey.AutoIncrement = true;
                }
                ret.ColumnConstraints.Add(primKey);
            }

            if (command.Default != null)
            {
                ret.ColumnConstraints.Add(new DefaultConstraintNode { Value = ConvertToSqlValue(command.Default) });
            }

            if (command.IsNotNull)
            {
                ret.ColumnConstraints.Add(new NotNullConstraintNode());
            }
            else if (command.Default == null && !command.IsPrimaryKey && !command.IsUnique)
            {
                ret.ColumnConstraints.Add(new DefaultConstraintNode { Value = "NULL" });
            }

            if (command.IsUnique)
            {
                ret.ColumnConstraints.Add(new UniqueConstraintNode());
            }

            return ret;
        }

        public string ConvertToSqlValue(object value) {
            if (value == null) {
                return "null";
            }

            TypeCode typeCode = Type.GetTypeCode(value.GetType());
            switch (typeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.String:
                case TypeCode.Char:
                    return String.Concat("'", Convert.ToString(value).Replace("'", "''"), "'");
                case TypeCode.Boolean:
                    return _dialect.ToBooleanValueString((bool)value);
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
                case TypeCode.DateTime:
                    return String.Concat("'", Convert.ToString(value, CultureInfo.InvariantCulture), "'");
            }

            return "null";
        }
    }

    internal class OrchardToSQLiteAdapter {
        private readonly ShellSettings _shellSettings;
        private readonly Dialect _dialect;

        internal OrchardToSQLiteAdapter(ShellSettings shellSettings, Dialect dialect) {
            _shellSettings = shellSettings;
            _dialect = dialect;
        }


        private OcfAction.TableCommand Visit(TableCommand command) {
            if (command is AddColumnCommand) {
                return Visit(command as AddColumnCommand);
            }
            if (command is DropColumnCommand) {
                return Visit(command as DropColumnCommand);
            }
            if (command is AddIndexCommand) {
                return Visit(command as AddIndexCommand);
            }
            if (command is DropIndexCommand) {
                return Visit(command as DropIndexCommand);
            }

            if (command is AlterColumnCommand) {
                return Visit(command as AlterColumnCommand);
            }

            return null;
        }

        public OcfAction.AlterTableCommand Visit(AlterTableCommand command) {
            var output = new OcfAction.AlterTableCommand(PrefixTableName(command.Name));

            foreach (var tc in command.TableCommands) {
                output.TableCommands.Add(Visit(tc));
            }

            return output;

        }

        public OcfAction.AddColumnCommand Visit(AddColumnCommand command) {
            var output = new OcfAction.AddColumnCommand(PrefixTableName(command.TableName), command.ColumnName);
            
            output.WithType(GetTypeName(command.DbType));

            if (command.IsNotNull) {
                output.NotNull();
            }
            else {
                output.Nullable();
            }
            

            if (command.IsUnique) {
                output.Unique();
            }
            if (command.IsPrimaryKey) {
                output.PrimaryKey();
            }

            if (command.IsIdentity) {
                output.Identity();
            }

            output.WithDefault(command.Default);


            return output;
        }

        public OcfAction.DropColumnCommand Visit(DropColumnCommand command) {
            return new OcfAction.DropColumnCommand(PrefixTableName(command.TableName), command.ColumnName);
        }

        public OcfAction.AlterColumnCommand Visit(AlterColumnCommand command) {
            var output = new OcfAction.AlterColumnCommand(PrefixTableName(command.TableName), command.ColumnName);
            if (command.DbType != DbType.Object) {
                output.WithType(GetTypeName(command.DbType));
            }
            output.WithDefault(command.Default);

            return output;
        }

        public OcfAction.AddIndexCommand Visit(AddIndexCommand command) {
            return new OcfAction.AddIndexCommand(PrefixTableName(command.TableName), command.IndexName, command.ColumnNames);
        }

        public OcfAction.DropIndexCommand Visit(DropIndexCommand command) {
            return new OcfAction.DropIndexCommand(PrefixTableName(command.TableName), command.IndexName);
        }


        public OcfAction.AlterTableCommand Visit(DropForeignKeyCommand command) {
            var output = new OcfAction.AlterTableCommand(PrefixTableName(command.SrcTable));
            output.DropForeignKey(PrefixTableName(command.Name));
            return output;
        }


        public OcfAction.AlterTableCommand Visit(CreateForeignKeyCommand command) {
            var output = new OcfAction.AlterTableCommand(PrefixTableName(command.SrcTable));
            output.CreateForeignKey(PrefixTableName(command.Name), command.SrcColumns, PrefixTableName(command.DestTable), command.DestColumns);

            return output;
        }




        private string PrefixTableName(string tableName) {
            return SQLiteCommandInterpreter.PrefixTableName(_shellSettings, tableName);
        }

        public string GetTypeName(DbType dbType) {
            return _dialect.GetTypeName(new SqlType(dbType));
        }

        

    }




}
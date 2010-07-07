using System.Data;
using Autofac;
using NHibernate;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;
using Orchard.Tests.ContentManagement;
using System.IO;

namespace Orchard.Tests.DataMigration {
    [TestFixture]
    public class SchemaBuilderTests {
        private IContainer _container;
        private ISessionFactory _sessionFactory;
        private string _databaseFileName;
        private SchemaBuilder _schemaBuilder;
        private DefaultDataMigrationInterpreter _interpreter;

        [SetUp]
        public void Setup() {
            _databaseFileName = Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                _databaseFileName);

            var builder = new ContainerBuilder();

            builder.RegisterInstance(new ShellSettings { DataTablePrefix = "TEST_", DataProvider = "SQLite" });

            var session = _sessionFactory.OpenSession();
            builder.RegisterType<DefaultDataMigrationInterpreter>().As<IDataMigrationInterpreter>();
            builder.RegisterInstance(new DefaultContentManagerTests.TestSessionLocator(session)).As<ISessionLocator>();
            builder.RegisterInstance(new ShellSettings { DataProvider = "SQLite", DataTablePrefix = "TEST_" }).As<ShellSettings>();
            builder.RegisterType<SqLiteCommandInterpreter>().As<ICommandInterpreter>();
            _container = builder.Build();

            _interpreter = _container.Resolve<IDataMigrationInterpreter>() as DefaultDataMigrationInterpreter;
            _schemaBuilder = new SchemaBuilder(_interpreter);
        }

        [Test]
        public void AllMethodsShouldBeCalledSuccessfully() {

            _schemaBuilder = new SchemaBuilder(new NullInterpreter());

            _schemaBuilder
                .CreateTable("User", table => table
                    .ContentPartRecord()
                    .Column("Id", DbType.Int32, column => column.PrimaryKey())
                    .Column("Firstname", DbType.String, column => column.WithLength(255))
                    .Column("Lastname", DbType.String, column => column.WithPrecision(0).WithScale(1)))
                .CreateTable("Address", table => table
                    .VersionedContentPartRecord()
                    .Column("City", DbType.String)
                    .Column("ZIP", DbType.Int32, column => column.Unique())
                    .Column("UserId", DbType.Int32, column => column.NotNull()))
                .CreateForeignKey("User_Address", "User", new[] { "UserId" }, "User", new[] { "Id" })
                .AlterTable("User", table => table
                    .AddColumn("Age", DbType.Int32))
                .AlterTable("User", table => table
                    .DropColumn("Lastname"))
                .AlterTable("User", table => table
                    .CreateIndex("IDX_XYZ", "NickName"))
                .AlterTable("User", table => table
                    .DropIndex("IDX_XYZ"))
                .DropForeignKey("Address", "User_Address")
                .DropTable("Address")
                .ExecuteSql("drop database", statement => statement.ForProvider("SQLite"))
                .ExecuteSql("DROP DATABASE", statement => statement.ForProvider("SQLServer"));
        }

        [Test]
        public void CreateCommandShouldBeHandled() {
            
            _schemaBuilder
                .CreateTable("User", table => table
                    .Column("Id", DbType.Int32, column => column.PrimaryKey())
                    .Column("Firstname", DbType.String, column => column.WithLength(255))
                    .Column("Lastname", DbType.String, column => column.WithLength(100).NotNull())
                    .Column("SN", DbType.AnsiString, column => column.WithLength(40).Unique())
                    .Column("Salary", DbType.Decimal, column => column.WithPrecision(9).WithScale(2))
                    .Column("Gender", DbType.Decimal, column => column.WithDefault("''"))
                    );
        }

        [Test]
        public void DropTableCommandShouldBeHandled() {

            _schemaBuilder
                .DropTable("User");
        }

        [Test]
        public void CustomSqlStatementsShouldBeHandled() {

            _schemaBuilder
                .ExecuteSql("select 1");
        }

        [Test]
        public void AlterTableCommandShouldBeHandled() {

            _schemaBuilder
                .CreateTable("User", table => table
                    .Column("Firstname", DbType.String, column => column.WithLength(255))
                    .Column("Lastname", DbType.String, column => column.WithLength(100).NotNull()))
                .AlterTable("User", table => table
                    .AddColumn("Age", DbType.Int32))
                .AlterTable("User", table => table
                    .AlterColumn("Lastname", column => column.WithDefault("'John'")))
                .AlterTable("User", table => table
                    .DropColumn("Firstname")
                );
        }

        [Test]
        public void ForeignKeyShouldBeCreatedAndRemoved() {

            _schemaBuilder
                .CreateTable("User", table => table
                    .Column("Id", DbType.Int32, column => column.PrimaryKey())
                    .Column("Firstname", DbType.String, column => column.WithLength(255))
                    .Column("Lastname", DbType.String, column => column.WithPrecision(0).WithScale(1)))
                .CreateTable("Address", table => table
                    .Column("City", DbType.String)
                    .Column("ZIP", DbType.Int32, column => column.Unique())
                    .Column("UserId", DbType.Int32, column => column.NotNull()))
                .CreateForeignKey("User_Address", "User", new[] { "UserId" }, "User", new[] { "Id" })
                .DropForeignKey("User", "User_Address");

        }
    }
}

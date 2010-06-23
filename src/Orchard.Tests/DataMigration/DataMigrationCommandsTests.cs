using System.Data;
using NUnit.Framework;
using Orchard.DataMigration.Schema;

namespace Orchard.Tests.DataMigration {
    [TestFixture]
    public class DataMigrationCommandsTests {

        [Test]
        public void AllMethodsShouldBeCalledSuccessfully() {
            var schemaBuilder = new SchemaBuilder("TEST_");

            schemaBuilder
                .CreateTable("User", table => table
                    .ContentPartRecord()
                    .Column("Id", DbType.Int32, column => column.PrimaryKey())
                    .Column("Firstname", DbType.String, column => column.Length(255))
                    .Column("Lastname", DbType.String, column => column.Precision(0).Scale(1))
                    .ForeignKey("User_Address", fk => fk.On("Id", "Address", "UserId")))
                .CreateTable("Address", table => table
                    .VersionedContentPartRecord()
                    .Column("City", DbType.String)
                    .Column("ZIP", DbType.Int32, column => column.Unique())
                    .Column("UserId", DbType.Int32, column => column.NotNull()))
                .AlterTable("User", table => table
                    .AddColumn("Age", DbType.Int32)
                    .AlterColumn("Lastname", column => column.Default("John"))
                    .AlterColumn("Lastname", column => column.Rename("John"))
                    .DropColumn("Lastname")
                    .CreateIndex("IDX_XYZ", "NickName")
                    .DropIndex("IDX_XYZ")
                    .AddForeignKey("FKL", fk => fk.On("Id", "A", "Id").On("Id", "B", "Id") )
                    .DropForeignKey("FKL"))
                .DropTable("Address")
                .ExecuteSql("DROP DATABASE", statement => statement.ForDialect("SQLite").ForDialect("MsSqlServer2008"));
        }
    }
}

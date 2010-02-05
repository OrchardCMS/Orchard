using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Orchard.Data.Migrations;
using Orchard.Environment;
using Orchard.Tests.Records;

namespace Orchard.Tests.Data.Migrations {
    [TestFixture]
    public class DatabaseMigrationManagerTests {
        private string _tempDataFolder;

        [SetUp]
        public void Init() {
            var tempFilePath = Path.GetTempFileName();
            File.Delete(tempFilePath);
            Directory.CreateDirectory(tempFilePath);
            _tempDataFolder = tempFilePath;
        }

        [TearDown]
        public void Term() {
            try { Directory.Delete(_tempDataFolder, true); }
            catch (IOException) { }
        }

        private static void CreateSqlServerDatabase(string databasePath) {
            var databaseName = Path.GetFileNameWithoutExtension(databasePath);
            using (var connection = new SqlConnection(
                "Data Source=.\\SQLEXPRESS;Initial Catalog=tempdb;Integrated Security=true;User Instance=True;")) {
                connection.Open();
                using (var command = connection.CreateCommand()) {
                    command.CommandText =
                        "CREATE DATABASE " + databaseName +
                        " ON PRIMARY (NAME=" + databaseName +
                        ", FILENAME='" + databasePath.Replace("'", "''") + "')";
                    command.ExecuteNonQuery();

                    command.CommandText =
                        "EXEC sp_detach_db '" + databaseName + "', 'true'";
                    command.ExecuteNonQuery();
                }
            }
        }

        [Test]
        public void MigrationManagerShouldCreateEmptySQLiteDatabaseAtGivenLocation() {
            var manager = (IDatabaseMigrationManager)new DatabaseMigrationManager();
            var coordinator = manager.CreateCoordinator("SQLite", _tempDataFolder, "");

            coordinator.CreateDatabase();

            Assert.That(File.Exists(Path.Combine(_tempDataFolder, "Orchard.db")), Is.True);
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void MigrationManagerShouldNotImplementTheCreationOfSqlServer() {
            var manager = (IDatabaseMigrationManager)new DatabaseMigrationManager();
            var coordinator = manager.CreateCoordinator("SqlServer", _tempDataFolder, "");

            coordinator.CreateDatabase();

        }

        [Test]
        public void CanConnectShouldBeFalseWhenSqlServerIsInvalid() {
            var manager = (IDatabaseMigrationManager)new DatabaseMigrationManager();
            var coordinator = manager.CreateCoordinator("SqlServer", _tempDataFolder, "Data Source=.\\SQLEXPRESS;Initial Catalog=Hello");
            Assert.That(coordinator.CanConnect(), Is.False);
        }

        [Test]
        public void CanConnectShouldBeTrueWhenValidSqlServerMdfIsTargetted() {
            var databasePath = Path.Combine(_tempDataFolder, "Orchard.mdf");
            CreateSqlServerDatabase(databasePath);

            var manager = (IDatabaseMigrationManager)new DatabaseMigrationManager();
            var coordinator = manager.CreateCoordinator("SqlServer", _tempDataFolder, "Data Source=.\\SQLEXPRESS;AttachDbFileName=" + databasePath + ";Integrated Security=True;User Instance=True;");
            Assert.That(coordinator.CanConnect(), Is.True);
        }

        [Test]
        public void SQLiteSchemaShouldBeGeneratedAndUsable() {
            var manager = (IDatabaseMigrationManager) new DatabaseMigrationManager();
            var coordinator = manager.CreateCoordinator("SQLite", _tempDataFolder, "");

            var recordDescriptors = new[] {
                new RecordDescriptor {Prefix = "Hello", Type = typeof (Foo)}
            };

            coordinator.UpdateSchema(recordDescriptors);

            var sessionFactory = coordinator.BuildSessionFactory(recordDescriptors);

            var session = sessionFactory.OpenSession();
            var foo = new Foo {Name = "hi there"};
            session.Save(foo);
            session.Flush();
            session.Close();

            Assert.That(foo, Is.Not.EqualTo(0));

            sessionFactory.Close();

        }

        [Test]
        public void SqlServerSchemaShouldBeGeneratedAndUsable() {
            var databasePath = Path.Combine(_tempDataFolder, "Orchard.mdf");
            CreateSqlServerDatabase(databasePath);

            var manager = (IDatabaseMigrationManager)new DatabaseMigrationManager();
            var coordinator = manager.CreateCoordinator("SqlServer", _tempDataFolder, "Data Source=.\\SQLEXPRESS;AttachDbFileName=" + databasePath + ";Integrated Security=True;User Instance=True;");

            var recordDescriptors = new[] {
                new RecordDescriptor {Prefix = "Hello", Type = typeof (Foo)}
            };

            coordinator.UpdateSchema(recordDescriptors);

            var sessionFactory = coordinator.BuildSessionFactory(recordDescriptors);

            var session = sessionFactory.OpenSession();
            var foo = new Foo { Name = "hi there" };
            session.Save(foo);
            session.Flush();
            session.Close();

            Assert.That(foo, Is.Not.EqualTo(0));

            sessionFactory.Close();
        }
    }
}

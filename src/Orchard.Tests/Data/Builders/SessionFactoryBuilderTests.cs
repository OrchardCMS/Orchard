using System.Data.SqlClient;
using System.IO;
using NUnit.Framework;
using Orchard.Data.Builders;
using Orchard.Environment;
using Orchard.Tests.Records;

namespace Orchard.Tests.Data.Builders {
    [TestFixture]
    public class SessionFactoryBuilderTests {
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
        public void SQLiteSchemaShouldBeGeneratedAndUsable() {
            var recordDescriptors = new[] {
                                              new RecordDescriptor_Obsolete {Prefix = "Hello", Type = typeof (FooRecord)}
                                          };
            var manager = (ISessionFactoryBuilder)new SessionFactoryBuilder();
            var sessionFactory = manager.BuildSessionFactory(new SessionFactoryParameters {
                                                                                              Provider = "SQLite",
                                                                                              DataFolder = _tempDataFolder,
                                                                                              UpdateSchema = true,
                                                                                              RecordDescriptors = recordDescriptors
                                                                                          });


            var session = sessionFactory.OpenSession();
            var foo = new FooRecord { Name = "hi there" };
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

            var recordDescriptors = new[] {
                                              new RecordDescriptor_Obsolete {Prefix = "Hello", Type = typeof (FooRecord)}
                                          };

            var manager = (ISessionFactoryBuilder)new SessionFactoryBuilder();
            var sessionFactory = manager.BuildSessionFactory(new SessionFactoryParameters {
                                                                                              Provider = "SQLite",
                                                                                              DataFolder = _tempDataFolder,
                                                                                              ConnectionString = "Data Source=.\\SQLEXPRESS;AttachDbFileName=" + databasePath + ";Integrated Security=True;User Instance=True;",
                                                                                              UpdateSchema = true,
                                                                                              RecordDescriptors = recordDescriptors,
                                                                                          });



            var session = sessionFactory.OpenSession();
            var foo = new FooRecord { Name = "hi there" };
            session.Save(foo);
            session.Flush();
            session.Close();

            Assert.That(foo, Is.Not.EqualTo(0));

            sessionFactory.Close();
        }
    }
}
using System;
using NUnit.Framework;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Tests.Records;

namespace Orchard.Tests.Data {
    [TestFixture]
    public class ProvidersTests {

        [Test]
        public void SqlCeShouldHandleBigFields() {
           
            var recordDescriptors = new[] {
                                              new RecordBlueprint {TableName = "Big", Type = typeof (BigRecord)}
                                          };

            ProviderUtilities.RunWithSqlCe(recordDescriptors,
                sessionFactory => {
                    var session = sessionFactory.OpenSession();
                    var foo = new BigRecord { Body = new String('x', 10000), Banner = new byte[10000]};
                    session.Save(foo);
                    session.Flush();
                    session.Close();

                    session = sessionFactory.OpenSession();
                    foo = session.Get<BigRecord>(foo.Id);
                    session.Close();

                    Assert.That(foo, Is.Not.Null);
                    Assert.That(foo.Body, Is.EqualTo(new String('x', 10000)));
                    Assert.That(foo.Banner.Length, Is.EqualTo(10000));
                });
        }


        [Test]
        public void SqlServerShouldHandleBigFields() {

            var recordDescriptors = new[] {
                                              new RecordBlueprint {TableName = "Big", Type = typeof (BigRecord)}
                                          };

            ProviderUtilities.RunWithSqlServer(recordDescriptors,
                sessionFactory => {
                    var session = sessionFactory.OpenSession();
                    var foo = new BigRecord { Body = new String('x', 10000), Banner = new byte[10000] };
                    session.Save(foo);
                    session.Flush();
                    session.Close();

                    session = sessionFactory.OpenSession();
                    foo = session.Get<BigRecord>(foo.Id);
                    session.Close();

                    Assert.That(foo, Is.Not.Null);
                    Assert.That(foo.Body, Is.EqualTo(new String('x', 10000)));
                    Assert.That(foo.Banner.Length, Is.EqualTo(10000));

                });
        }
    }
}
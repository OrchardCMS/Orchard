using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using NUnit.Framework;
using Orchard.DisplayManagement;

namespace Orchard.Tests.DisplayManagement {
    [TestFixture]
    public class DefaultShapeTableFactoryTests {
        static IShapeTableFactory CreateShapeTableFactory(Action<ContainerBuilder> config) {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultShapeTableFactory>().As<IShapeTableFactory>();
            config(builder);
            var container = builder.Build();
            return container.Resolve<IShapeTableFactory>();
        }

        [Test]
        public void ShapeTableRecognizesMethodNames() {
            var stf = CreateShapeTableFactory(cfg => cfg.RegisterType<Test>().As<IShapeProvider>());
            var shapeTable = stf.CreateShapeTable();
            Assert.That(shapeTable.Entries.Count(), Is.EqualTo(2));
            Assert.That(shapeTable.Entries.ContainsKey("Pager"));
            Assert.That(shapeTable.Entries.ContainsKey("Email"));
        }

        public class Test : IShapeProvider {
            public void Pager() {
            }

            public void Email(string text, string address) {
            }
        }
    }


}

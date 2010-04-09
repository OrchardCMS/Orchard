using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using NUnit.Framework;
using Orchard.Commands;

namespace Orchard.Tests.Commands {
    [TestFixture]
    public class CommandManagerTests {
        private ICommandManager _manager;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<CommandManager>().As<ICommandManager>();
            builder.RegisterType<MyCommand>().As<ICommandHandler>();
            builder.RegisterModule(new CommandModule());
            var container = builder.Build();

            _manager = container.Resolve<ICommandManager>();
        }

        [Test]
        public void ManagerCanRunACommand() {
            var context = new CommandParameters { Arguments = new string[] { "FooBar" } };
            _manager.Execute(context);
            Assert.That(context.Output, Is.EqualTo("success!"));
        }

        [Test]
        public void ManagerCanRunACompositeCommand() {
            var context = new CommandParameters { Arguments = ("Foo Bar Bleah").Split(' ') };
            _manager.Execute(context);
            Assert.That(context.Output, Is.EqualTo("Bleah"));
        }

        public class MyCommand : DefaultOrchardCommandHandler {

            public string FooBar() {
                return "success!";
            }

            public string Foo_Bar(string bleah) {
                return bleah;
            }
        }
    }
}

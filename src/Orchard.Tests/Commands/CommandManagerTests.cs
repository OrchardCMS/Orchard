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
            var container = builder.Build();

            _manager = container.Resolve<ICommandManager>();
        }

        [Test]
        public void ManagerCanRunACommand() {
            _manager.Execute(new CommandContext { Command = "foo bar" });
        }
    }
}

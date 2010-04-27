using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Orchard.Commands;

namespace Orchard.Tests.Commands {
    [TestFixture]
    public class CommandHandlerDescriptorBuilderTests {
        [Test]
        public void BuilderShouldCreateDescriptor() {
            var builder = new CommandHandlerDescriptorBuilder();
            var descriptor = builder.Build(typeof(MyCommand));
            Assert.That(descriptor, Is.Not.Null);
            Assert.That(descriptor.Commands.Count(), Is.EqualTo(4));
            Assert.That(descriptor.Commands.Single(d => d.Name == "FooBar"), Is.Not.Null);
            Assert.That(descriptor.Commands.Single(d => d.Name == "FooBar").MethodInfo, Is.EqualTo(typeof(MyCommand).GetMethod("FooBar")));
            Assert.That(descriptor.Commands.Single(d => d.Name == "MyCommand"), Is.Not.Null);
            Assert.That(descriptor.Commands.Single(d => d.Name == "MyCommand").MethodInfo, Is.EqualTo(typeof(MyCommand).GetMethod("FooBar2")));
            Assert.That(descriptor.Commands.Single(d => d.Name == "Foo Bar"), Is.Not.Null);
            Assert.That(descriptor.Commands.Single(d => d.Name == "Foo Bar").MethodInfo, Is.EqualTo(typeof(MyCommand).GetMethod("Foo_Bar")));
            Assert.That(descriptor.Commands.Single(d => d.Name == "Foo_Bar"), Is.Not.Null);
            Assert.That(descriptor.Commands.Single(d => d.Name == "Foo_Bar").MethodInfo, Is.EqualTo(typeof(MyCommand).GetMethod("Foo_Bar3")));
        }

        public class MyCommand : DefaultOrchardCommandHandler {
            public void FooBar() {
            }

            [CommandName("MyCommand")]
            public void FooBar2() {
            }

            public void Foo_Bar() {
            }

            [CommandName("Foo_Bar")]
            public void Foo_Bar3() {
            }
        }

        [Test]
        public void BuilderShouldReturnPublicMethodsOnly() {
            var builder = new CommandHandlerDescriptorBuilder();
            var descriptor = builder.Build(typeof(PublicMethodsOnly));
            Assert.That(descriptor, Is.Not.Null);
            Assert.That(descriptor.Commands.Count(), Is.EqualTo(1));
            Assert.That(descriptor.Commands.Single(d => d.Name == "Method"), Is.Not.Null);
        }

        public class PublicMethodsOnly {
            public bool Bar { get; set; }   // no accessors
            public bool Field = true;       // no field
            public event Action<int> Event; // no event adder, remover, etc.

            // no private method
            private void Blah() {
            }

            public void Method() {
            }
        }

    }
}

using System.Linq;
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

#pragma warning disable 660,661
        public class PublicMethodsOnly {
#pragma warning restore 660,661
            public bool Bar { get; set; }   // no accessors
            public bool Field = true;       // no field

            // no private method
            private void Blah() {
            }

            // no private method
            public static void Foo() {
            }

            // no operator
            public static bool operator ==(PublicMethodsOnly a, PublicMethodsOnly b) {
                return false;
            }

            public static bool operator !=(PublicMethodsOnly a, PublicMethodsOnly b) {
                return false;
            }

            public void Method() {
            }
        }

    }
}

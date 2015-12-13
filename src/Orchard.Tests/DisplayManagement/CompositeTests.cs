using NUnit.Framework;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Tests.DisplayManagement {
    [TestFixture]
    public class CompositeTests {

        [Test]
        public void CompositesShouldNotOverrideExistingMembers() {
            var composite = new Animal {Color = "Pink"};

            Assert.That(composite.Color, Is.EqualTo("Pink"));
        }

        [Test]
        public void CompositesShouldNotOverrideExistingMembersWhenUsedAsDynamic() {
            dynamic composite = new Animal();

            composite.Color = "Pink";
            Assert.That(composite.Color, Is.EqualTo("Pink"));
        }

        [Test]
        public void CompositesShouldAccessUnknownProperties() {
            dynamic composite = new Animal();

            composite.Fake = 42;
            Assert.That(composite.Fake, Is.EqualTo(42));
        }

        [Test]
        public void CompositesShouldAccessUnknownPropertiesByIndex() {
            dynamic composite = new Animal();

            composite["Fake"] = 42;
            Assert.That(composite["Fake"], Is.EqualTo(42));
        }

        [Test]
        public void CompositesShouldAccessKnownPropertiesByIndex() {
            dynamic composite = new Animal();

            composite["Pink"] = "Pink";
            Assert.That(composite["Pink"], Is.EqualTo("Pink"));
        }

        [Test]
        public void ChainProperties() {
            dynamic foo = new Animal();
            foo.Bar("bar");

            Assert.That(foo.Bar, Is.EqualTo("bar"));
            Assert.That(foo.Bar == null, Is.False);
        }


        [Test]
        public void DuckTyping() {
            dynamic foo = new Animal();
            foo.Size(42);

            ISized sized = foo;
            
            Assert.That(sized.Size, Is.EqualTo(42));
        }
    }

    public class Animal : Composite {
        public string Kind { get; set; }
        public string Color { get; set; }
    }

    public interface ISized {
        int Size { get; set; }
    }
}

using NUnit.Framework;
using Orchard.Projections.Settings;
using System.Collections.Generic;
using System.IO;
using Orchard.Projections.Settings.Serialization;

namespace Orchard.Projections.Tests.Settings {
    [TestFixture]
    public class SettingsTests {
        [Test]
        public void ShouldRemoveMember() {
            dynamic e = new SObject();
            e.Foo = "Bar";
            Assert.That(e, Is.Not.Empty);
            Assert.That(e.Foo, Is.EqualTo("Bar"));

            e.Foo = null;
            Assert.That(e, Is.Empty);
        }

        [Test]
        public void ShouldSupportFactoryInvocation() {
            var e = SObject.New();

            e.Foo = "Bar";
            Assert.That(e["Foo"], Is.EqualTo("Bar"));
            Assert.That(e.Foo, Is.EqualTo("Bar"));
        }

        [Test]
        public void ShouldAddDynamicProperties() {
            dynamic e = new SObject();
            e.Foo = "Bar";
            Assert.That(e["Foo"], Is.EqualTo("Bar"));
            Assert.That(e.Foo, Is.EqualTo("Bar"));
        }

        [Test]
        public void UnknownPropertiesShouldBeNull() {
            dynamic e = new SObject();
            Assert.That((object)e["Foo"], Is.EqualTo(null));
            Assert.That((object)e.Foo, Is.EqualTo(null));
        }

        [Test]
        public void ShouldAddDynamicObjects() {
            dynamic e = new SObject();
            e.Address = new SObject();
            
            e.Address.Street = "One Microsoft Way";
            Assert.That(e["Address"]["Street"], Is.EqualTo("One Microsoft Way"));
            Assert.That(e.Address.Street, Is.EqualTo("One Microsoft Way"));
        }

        public void ShouldAddArraysOfAnonymousObject() {
            dynamic e = new SObject();

            e.Foos = new[] { new { Foo1 = "Bar1", Foo2 = "Bar2" } };
            Assert.That(e.Foos[0].Foo1, Is.EqualTo("Bar1"));
            Assert.That(e.Foos[0].Foo2, Is.EqualTo("Bar2"));
        }

        public void ShouldAddAnonymousObject() {
            dynamic e = new SObject();

            e.Foos = new { Foo1 = "Bar1", Foo2 = "Bar2" };
            Assert.That(e.Foos.Foo1, Is.EqualTo("Bar1"));
            Assert.That(e.Foos.Foo2, Is.EqualTo("Bar2"));
        }

        [Test]
        public void ShouldAddArrays() {
            dynamic e = new SObject();
            e.Owners = new[] { "Steve", "Bill" };
            Assert.That(e.Owners[0], Is.EqualTo("Steve"));
            Assert.That(e.Owners[1], Is.EqualTo("Bill"));
        }

        [Test]
        public void ShouldBeEnumerable() {
            dynamic e = new SObject();
            e.Address = new SObject();

            e.Address.Street = "One Microsoft Way";
            e.Foos = new[] { new { Foo1 = "Bar1", Foo2 = "Bar2" } };
            e.Owners = new[] { "Steve", "Bill" };

            // IEnumerable
            Assert.That(e, Has.Some.Matches<KeyValuePair<string, object>>(x => x.Key == "Address"));
            Assert.That(e, Has.Some.Matches<KeyValuePair<string, object>>(x => x.Key == "Owners"));
            Assert.That(e, Has.Some.Matches<KeyValuePair<string, object>>(x => x.Key == "Foos"));
        }

        [Test]
        public void ShouldSerializeAndDeserialize() {
            dynamic e = new SObject();
            
            e.Foo = "Bar";
            
            e.Address = new SObject();
            e.Address.Street = "One Microsoft Way";
            e.Owners = new[] { "Steve", "Bill" };
            e.Foos = new[] { new { Foo1 = "Bar1", Foo2 = "Bar2" } };

            string xml1;

            var serializer = new XmlSettingsSerializer();
            using (var sw = new StringWriter()) {
                serializer.Serialize(sw, e);
                xml1 = sw.ToString();
            }

            dynamic clone;

            using (var sr = new StringReader(xml1)) {
                clone = serializer.Deserialize(sr);
            }

            string xml2;

            using (var sw = new StringWriter()) {
                serializer.Serialize(sw, clone);
                xml2 = sw.ToString();
            }

            Assert.That(xml1, Is.EqualTo(xml2));
        }

        [Test]
        public void MergeShouldOverwriteExistingProperties() {
            var o1 = SObject.New();
            o1.Foo = "Foo1";
            o1.Bar = "Bar1";

            var o2 = SObject.New();
            o2.Foo = "Foo2";
            o2.Baz = "Baz2";

            var o3 = o1 & o2;

            Assert.That(o3.Foo, Is.EqualTo("Foo2"));
            Assert.That(o3.Bar, Is.EqualTo("Bar1"));
            Assert.That(o3.Baz, Is.EqualTo("Baz2"));
        }

        [Test]
        public void MergeShouldConcatenateArrays() {
            var o1 = SObject.New();
            o1.Foo = new[] { "a", "b" };

            var o2 = SObject.New();
            o2.Foo = new[] { "c", "d" };

            var o3 = o1 & o2;

            Assert.That(o3.Foo[0], Is.EqualTo("a"));
            Assert.That(o3.Foo[1], Is.EqualTo("b"));
            Assert.That(o3.Foo[2], Is.EqualTo("c"));
            Assert.That(o3.Foo[3], Is.EqualTo("d"));
        }
    }
}

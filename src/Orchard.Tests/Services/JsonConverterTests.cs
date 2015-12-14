using System;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Orchard.Services;

namespace Orchard.Tests.Services {

    [TestFixture]
    public class JsonConverterTests {
        [Test]
        public void ShouldConvertPrimitiveTypesToJSon() {
            var converter = new DefaultJsonConverter();

            Assert.That(converter.Serialize(12), Is.EqualTo("12"));
            Assert.That(converter.Serialize(true), Is.EqualTo("true"));
            Assert.That(converter.Serialize(12.345), Is.EqualTo("12.345"));
            Assert.That(converter.Serialize("string"), Is.EqualTo("\"string\""));
            Assert.That(converter.Serialize(new DateTime(2013, 1, 31, 0, 0, 0, DateTimeKind.Utc)), Is.EqualTo("\"2013-01-31T00:00:00Z\""));
        }

        [Test]
        public void ShouldConvertAnonymousTypeToJSon() {
            dynamic d = JObject.Parse("{number:1000, str:'string', array: [1,2,3,4,5,6]}");

            Assert.That((int)d.number, Is.EqualTo(1000));
            Assert.That((int)d["number"], Is.EqualTo(1000));
            Assert.That((int)d.array.Count, Is.EqualTo(6));
        }

        [Test]
        public void ShouldConvertWellKnownTypeToJSon() {
            var converter = new DefaultJsonConverter();
            string result = converter.Serialize(new Animal { Age = 12, Name = "Milou" });
            var o = converter.Deserialize<Animal>(result);

            Assert.That(o.Age, Is.EqualTo(12));
            Assert.That(o.Name, Is.EqualTo("Milou"));
        }


        public class Animal {
            public int Age { get; set; }
            public string Name { get; set; }
        }
    }
}


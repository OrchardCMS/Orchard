using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Tests.Models.Stubs;

namespace Orchard.Tests.Models.Drivers {
    [TestFixture]
    public class ModelBuilderTests {
        [Test]
        public void BuilderShouldReturnWorkingModelWithTypeAndId() {
            var builder = new ContentItemBuilder("foo");
            var model = builder.Build();
            Assert.That(model.ContentType, Is.EqualTo("foo"));
        }

        [Test]
        public void IdShouldDefaultToZero() {
            var builder = new ContentItemBuilder("foo");
            var model = builder.Build();
            Assert.That(model.Id, Is.EqualTo(0));
        }

        [Test]
        public void WeldShouldAddPartToModel() {
            var builder = new ContentItemBuilder("foo");
            builder.Weld<Alpha>();
            var model = builder.Build();

            Assert.That(model.Is<Alpha>(), Is.True);
            Assert.That(model.As<Alpha>(), Is.Not.Null);
            Assert.That(model.Is<Beta>(), Is.False);
            Assert.That(model.As<Beta>(), Is.Null);
        }
    }
}


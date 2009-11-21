using NUnit.Framework;
using Orchard.Models;
using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Drivers {

    [TestFixture]
    public class ModelDriverTests {
        [Test]
        public void ModelDriverShouldUsePersistenceFilterToDelegateCreateAndLoad() {
            var modelDriver = new TestModelProvider();

            var contentItem = new ContentItem();
            var part = new TestModelPart();
            contentItem.Weld(part);

            ((IContentProvider)modelDriver).Creating(new CreateContentContext { ContentItem = contentItem });
            Assert.That(part.CreatingCalled, Is.True);
        }

        [Test]
        public void PartShouldBeAddedBasedOnSimplePredicate() {
            var modelDriver = new TestModelProvider();

            var builder = new ContentItemBuilder("testing");
            ((IContentProvider)modelDriver).Activating(new ActivatingContentContext { Builder = builder, ContentType = "testing" });
            var model = builder.Build();
            Assert.That(model.Is<TestModelPart>(), Is.True);
            Assert.That(model.As<TestModelPart>(), Is.Not.Null);
        }

        public class TestModelPart : ContentPart {
            public bool CreatingCalled { get; set; }
        }


        public class TestModelProvider : ContentProvider {
            public TestModelProvider() {
                Filters.Add(new ActivatingFilter<TestModelPart>(x => x == "testing"));
                Filters.Add(new TestModelStorageFilter());
            }
        }

        public class TestModelStorageFilter : StorageFilterBase<TestModelPart> {
            protected override void Creating(CreateContentContext context, TestModelPart instance) {
                instance.CreatingCalled = true;
            }
        }
    }
}


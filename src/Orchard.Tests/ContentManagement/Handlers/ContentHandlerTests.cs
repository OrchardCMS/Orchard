using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.Tests.ContentManagement.Handlers {

    [TestFixture]
    public class ContentHandlerTests {
        [Test]
        public void ModelDriverShouldUsePersistenceFilterToDelegateCreateAndLoad() {
            var modelDriver = new TestModelHandler();

            var contentItem = new ContentItem();
            var part = new TestModelPart();
            contentItem.Weld(part);

            ((IContentHandler)modelDriver).Creating(new CreateContentContext(contentItem));
            Assert.That(part.CreatingCalled, Is.True);
        }

        [Test]
        public void PartShouldBeAddedBasedOnSimplePredicate() {
            var modelDriver = new TestModelHandler();

            var builder = new ContentItemBuilder(new ContentTypeDefinitionBuilder().Named("testing").Build());
            ((IContentHandler)modelDriver).Activating(new ActivatingContentContext { Builder = builder, ContentType = "testing" });
            var model = builder.Build();
            Assert.That(model.Is<TestModelPart>(), Is.True);
            Assert.That(model.As<TestModelPart>(), Is.Not.Null);
        }

        public class TestModelPart : ContentPart {
            public bool CreatingCalled { get; set; }
        }


        public class TestModelHandler : ContentHandler {
            public TestModelHandler() {
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


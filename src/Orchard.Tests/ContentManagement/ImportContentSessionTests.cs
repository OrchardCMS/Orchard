using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Tests.ContentManagement {
    [TestFixture]
    public class ImportContentSessionTests {
        private ContentIdentity _testItemIdentity1;
        private ContentIdentity _testItemIdentity2;
        private ContentIdentity _testItemIdentity3;
        private ContentIdentity _testItemIdentity4;
        private ContentIdentity _testItemIdentity5;
        private Mock<IContentManager> _contentManager;

        #region Init
        [TestFixtureSetUp]
        public void TestInit() {
            _testItemIdentity1 = new ContentIdentity("/ItemId=1");
            _testItemIdentity2 = new ContentIdentity("/ItemId=2");
            _testItemIdentity3 = new ContentIdentity("/ItemId=3");
            _testItemIdentity4 = new ContentIdentity("/ItemId=4");
            _testItemIdentity5 = new ContentIdentity("/ItemId=5");
            var draftItem = new ContentItem { VersionRecord = new ContentItemVersionRecord { Id = 1234, Published = false, Latest = true, ContentItemRecord = new ContentItemRecord { Id = 1 } } };
            var publishedItem = new ContentItem { VersionRecord = new ContentItemVersionRecord { Id = 1234, Published = true, Latest = true, ContentItemRecord = new ContentItemRecord { Id = 1 } } };

            var draftItem5 = new ContentItem { VersionRecord = new ContentItemVersionRecord { Id = 1234, Published = false, Latest = true, ContentItemRecord = new ContentItemRecord { Id = 5 } } };
            var publishedItem5 = new ContentItem { VersionRecord = new ContentItemVersionRecord { Id = 1234, Published = true, Latest = true, ContentItemRecord = new ContentItemRecord { Id = 5 } } };

            _contentManager = new Mock<IContentManager>();
            _contentManager.Setup(m => m.Get(It.Is<int>(v => v == 1), It.Is<VersionOptions>(v => v.IsDraftRequired))).Returns(draftItem);
            _contentManager.Setup(m => m.Get(It.Is<int>(v => v == 1), It.Is<VersionOptions>(v => !v.IsDraftRequired))).Returns(publishedItem);

            _contentManager.Setup(m => m.Get(It.Is<int>(v => v == 5), It.Is<VersionOptions>(v => v.IsDraftRequired))).Returns(draftItem5);
            _contentManager.Setup(m => m.Get(It.Is<int>(v => v == 5), It.Is<VersionOptions>(v => !v.IsDraftRequired))).Returns(publishedItem5);

            _contentManager.Setup(m => m.GetItemMetadata(It.Is<IContent>(c => c.Id == 1))).Returns(new ContentItemMetadata { Identity = _testItemIdentity1 });
            _contentManager.Setup(m => m.GetItemMetadata(It.Is<IContent>(c => c.Id == 5))).Returns(new ContentItemMetadata { Identity = _testItemIdentity5 });

            _contentManager.Setup(m => m.New(It.IsAny<string>())).Returns(draftItem5);

            _contentManager.Setup(m => m.ResolveIdentity(It.Is<ContentIdentity>(id => id.Get("ItemId") == "1"))).Returns(publishedItem);
        }
        #endregion

        [Test]
        public void GetNextInBatchReturnsNullWhenNoItemsSet() {
            var importContentSession = new ImportContentSession(_contentManager.Object);

            Assert.That(importContentSession.GetNextInBatch(), Is.Null);
        }

        [Test]
        public void GetNextInBatchReturnsNullWhenInitializedButNoItemsSet() {
            var importContentSession = new ImportContentSession(_contentManager.Object);
            importContentSession.InitializeBatch(0, 20);
            Assert.That(importContentSession.GetNextInBatch(), Is.Null);
        }

        [Test]
        public void ItemsSetAndUninitializedReturnsAllItems() {
            var importContentSession = new ImportContentSession(_contentManager.Object);

            importContentSession.Set("/Id=One", "TestType");
            importContentSession.Set("/Id=Two", "TestType");
            importContentSession.Set("/Id=Three", "TestType");

            var comparer = new ContentIdentity.ContentIdentityEqualityComparer();
            Assert.That(comparer.Equals(importContentSession.GetNextInBatch(), new ContentIdentity("/Id=One")));
            Assert.That(comparer.Equals(importContentSession.GetNextInBatch(), new ContentIdentity("/Id=Two")));
            Assert.That(comparer.Equals(importContentSession.GetNextInBatch(), new ContentIdentity("/Id=Three")));
            Assert.That(importContentSession.GetNextInBatch(), Is.Null);
        }

        [Test]
        public void ItemsSetAndBatchInitialisedReturnsBatchedItems() {
            var importContentSession = new ImportContentSession(_contentManager.Object);

            importContentSession.Set("/Id=One", "TestType");
            importContentSession.Set("/Id=Two", "TestType");
            importContentSession.Set("/Id=Three", "TestType");
            importContentSession.Set("/Id=Four", "TestType");
            importContentSession.Set("/Id=Five", "TestType");

            importContentSession.InitializeBatch(1, 2);


            var comparer = new ContentIdentity.ContentIdentityEqualityComparer();
            Assert.That(comparer.Equals(importContentSession.GetNextInBatch(), new ContentIdentity("/Id=Two")));
            Assert.That(comparer.Equals(importContentSession.GetNextInBatch(), new ContentIdentity("/Id=Three")));
            Assert.That(importContentSession.GetNextInBatch(), Is.Null);

            importContentSession.InitializeBatch(2, 5);

            //item with "/Id=Three" should not be returned twice in the same session
            Assert.That(comparer.Equals(importContentSession.GetNextInBatch(), new ContentIdentity("/Id=Four")));
            Assert.That(comparer.Equals(importContentSession.GetNextInBatch(), new ContentIdentity("/Id=Five")));
            Assert.That(importContentSession.GetNextInBatch(), Is.Null);
        }

        [Test]
        public void GetItemExistsAndNoVersionOptionsReturnsPublishedItem() {
            var session = new ImportContentSession(_contentManager.Object);
            session.Set(_testItemIdentity1.ToString(), "TestContentType");
            var sessionItem = session.Get(_testItemIdentity1.ToString());
            Assert.IsNotNull(sessionItem);
            Assert.AreEqual(1, sessionItem.Id);
            Assert.IsTrue(sessionItem.IsPublished());
        }

        [Test]
        public void GetItemExistsAndLatestVersionOptionReturnsPublishedItem() {
            var session = new ImportContentSession(_contentManager.Object);
            session.Set(_testItemIdentity1.ToString(), "TestContentType");
            var sessionItem = session.Get(_testItemIdentity1.ToString());
            Assert.IsNotNull(sessionItem);
            Assert.AreEqual(1, sessionItem.Id);
            Assert.IsTrue(sessionItem.IsPublished());
        }

        [Test]
        public void GetItemExistsAndDraftRequiredVersionOptionReturnsDraft() {
            var session = new ImportContentSession(_contentManager.Object);
            session.Set(_testItemIdentity1.ToString(), "TestContentType");
            var sessionItem = session.Get(_testItemIdentity1.ToString(), VersionOptions.DraftRequired);
            Assert.IsNotNull(sessionItem);
            Assert.That(1, Is.EqualTo(sessionItem.Id));
            Assert.IsFalse(sessionItem.IsPublished());
        }

        [Test]
        public void GetNextInBatchInitialisedWithOneItemReturnsOneItemThenNull() {
            var session = new ImportContentSession(_contentManager.Object);
            session.Set(_testItemIdentity1.ToString(), "TestContentType");
            session.InitializeBatch(0, 1);
            var firstIdentity = session.GetNextInBatch();
            var secondIdentity = session.GetNextInBatch();

            var comparer = new ContentIdentity.ContentIdentityEqualityComparer();
            Assert.That(comparer.Equals(_testItemIdentity1, firstIdentity));
            Assert.That(secondIdentity, Is.Null);
        }


        [Test]
        public void GetNextInBatchInitialisedTwoBatchesReturnsItemsOnceEach() {
            var session = new ImportContentSession(_contentManager.Object);
            session.Set(_testItemIdentity1.ToString(), "TestContentType");
            session.Set(_testItemIdentity2.ToString(), "TestContentType");
            session.Set(_testItemIdentity3.ToString(), "TestContentType");
            session.Set(_testItemIdentity4.ToString(), "TestContentType");
            session.Set(_testItemIdentity5.ToString(), "TestContentType");

            session.InitializeBatch(0, 2);
            var firstIdentity = session.GetNextInBatch();
            //get later item as dependency
            var dependencyItem = session.Get(_testItemIdentity5.ToString(), VersionOptions.Latest);
            var dependencyIdentity = session.GetNextInBatch();
            var secondIdentity = session.GetNextInBatch();
            var afterBatch1 = session.GetNextInBatch();

            session.InitializeBatch(2, 2);
            var thirdIdentity = session.GetNextInBatch();
            var fourthdentity = session.GetNextInBatch();
            var afterBatch2 = session.GetNextInBatch();

            session.InitializeBatch(4, 2);
            var fifthIdentity = session.GetNextInBatch();
            var afterBatch3 = session.GetNextInBatch();

            var comparer = new ContentIdentity.ContentIdentityEqualityComparer();
            Assert.That(comparer.Equals(_testItemIdentity1, firstIdentity));
            Assert.That(comparer.Equals(_testItemIdentity5, dependencyIdentity));
            Assert.That(comparer.Equals(_testItemIdentity2, secondIdentity));
            Assert.That(afterBatch1, Is.Null);

            Assert.That(comparer.Equals(_testItemIdentity3, thirdIdentity));
            Assert.That(comparer.Equals(_testItemIdentity4, fourthdentity));
            Assert.That(afterBatch2, Is.Null);

            Assert.That(fifthIdentity, Is.Null); //already processed as dependency
            Assert.That(afterBatch3, Is.Null);
        }
    }
}
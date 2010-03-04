using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Handlers;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.Security;
using Orchard.Tests.Modules;

namespace Orchard.Core.Tests.Common.Providers {
    [TestFixture]
    public class CommonAspectProviderTests : DatabaseEnabledTestsBase {
        private Mock<IAuthenticationService> _authn;
        private Mock<IAuthorizationService> _authz;
        private Mock<IMembershipService> _membership;

        public override void Register(ContainerBuilder builder) {
            builder.Register<DefaultContentManager>().As<IContentManager>();
            builder.Register<TestHandler>().As<IContentHandler>();
            builder.Register<CommonAspectHandler>().As<IContentHandler>();

            _authn = new Mock<IAuthenticationService>();
            _authz = new Mock<IAuthorizationService>();
            _membership = new Mock<IMembershipService>();
            builder.Register(_authn.Object);
            builder.Register(_authz.Object);
            builder.Register(_membership.Object);

        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof(ContentTypeRecord), 
                                 typeof(ContentItemRecord), 
                                 typeof(ContentItemVersionRecord), 
                                 typeof(CommonRecord),
                                 typeof(CommonVersionRecord),
                             };
            }
        }

        [UsedImplicitly]
        class TestHandler : ContentHandler {
            public TestHandler() {
                Filters.Add(new ActivatingFilter<CommonAspect>("test-item"));
                Filters.Add(new ActivatingFilter<ContentPart<CommonVersionRecord>>("test-item"));
                Filters.Add(new ActivatingFilter<TestUser>("user"));
            }
        }

        class TestUser : ContentPart, IUser {
            public int Id { get { return 6655321; } }
            public string UserName {get { return "x"; }}
            public string Email { get { return "y"; } }
        }

        [Test]
        public void OwnerShouldBeNullAndZeroByDefault() {
            var contentManager = _container.Resolve<IContentManager>();
            var item = contentManager.Create<CommonAspect>("test-item", init => { });
            ClearSession();

            Assert.That(item.Owner, Is.Null);
            Assert.That(item.Record.OwnerId, Is.EqualTo(0));
        }

        [Test,Ignore("This testing is still being worked out")]
        public void OwnerShouldBeAuthenticatedUserIfAvailable() {
            var contentManager = _container.Resolve<IContentManager>();

            var user = contentManager.New<IUser>("user");
            _authn.Setup(x => x.GetAuthenticatedUser()).Returns(user);

            var item = contentManager.Create<CommonAspect>("test-item", init => { });
            
            ClearSession();

            Assert.That(item.Record.OwnerId, Is.EqualTo(6655321));
        }

        [Test]
        public void PublishingShouldSetPublishUtc() {
            var contentManager = _container.Resolve<IContentManager>();

            var createUtc = _clock.UtcNow;
            var item = contentManager.Create<ICommonAspect>("test-item", VersionOptions.Draft, init => { });

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.Null);
            
            _clock.Advance(TimeSpan.FromMinutes(1));
            var publishUtc = _clock.UtcNow;

            contentManager.Publish(item.ContentItem);

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.EqualTo(publishUtc));
        }

        [Test]
        public void VersioningItemShouldCreatedAndPublishedUtcValuesPerVersion() {
            var contentManager = _container.Resolve<IContentManager>();

            var createUtc = _clock.UtcNow;
            var item1 = contentManager.Create<ICommonAspect>("test-item", VersionOptions.Draft, init => { });

            Assert.That(item1.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item1.PublishedUtc, Is.Null);

            _clock.Advance(TimeSpan.FromMinutes(1));
            var publish1Utc = _clock.UtcNow;
            contentManager.Publish(item1.ContentItem);

            // db records need to be updated before demanding draft as item2 below
            _session.Flush();

            _clock.Advance(TimeSpan.FromMinutes(1));
            var draftUtc = _clock.UtcNow;
            var item2 = contentManager.GetDraftRequired<ICommonAspect>(item1.ContentItem.Id);

            _clock.Advance(TimeSpan.FromMinutes(1));
            var publish2Utc = _clock.UtcNow;
            contentManager.Publish(item2.ContentItem);

            // both instances non-versioned dates show it was created upfront
            Assert.That(item1.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item2.CreatedUtc, Is.EqualTo(createUtc));

            // both instances non-versioned dates show the most recent publish
            Assert.That(item1.PublishedUtc, Is.EqualTo(publish2Utc));
            Assert.That(item2.PublishedUtc, Is.EqualTo(publish2Utc));

            // version1 versioned dates show create was upfront and publish was oldest
            Assert.That(item1.VersionCreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item1.VersionPublishedUtc, Is.EqualTo(publish1Utc));

            // version2 versioned dates show create was midway and publish was most recent
            Assert.That(item2.VersionCreatedUtc, Is.EqualTo(draftUtc));
            Assert.That(item2.VersionPublishedUtc, Is.EqualTo(publish2Utc));
        }

        [Test]
        public void UnpublishShouldClearFlagButLeaveMostrecentPublishDatesIntact() {
            var contentManager = _container.Resolve<IContentManager>();

            var createUtc = _clock.UtcNow;
            var item = contentManager.Create<ICommonAspect>("test-item", VersionOptions.Draft, init => { });

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.Null);

            _clock.Advance(TimeSpan.FromMinutes(1));
            var publishUtc = _clock.UtcNow;
            contentManager.Publish(item.ContentItem);

            // db records need to be updated before seeking by published flags
            _session.Flush();

            _clock.Advance(TimeSpan.FromMinutes(1));
            var unpublishUtc = _clock.UtcNow;
            contentManager.Unpublish(item.ContentItem);

            // db records need to be updated before seeking by published flags
            _session.Flush();
            _session.Clear();

            var publishedItem = contentManager.Get<ICommonAspect>(item.ContentItem.Id, VersionOptions.Published);
            var latestItem = contentManager.Get<ICommonAspect>(item.ContentItem.Id, VersionOptions.Latest);
            var draftItem = contentManager.Get<ICommonAspect>(item.ContentItem.Id, VersionOptions.Draft);
            var allVersions = contentManager.GetAllVersions(item.ContentItem.Id);

            Assert.That(publishedItem, Is.Null);
            Assert.That(latestItem, Is.Not.Null);
            Assert.That(draftItem, Is.Not.Null);
            Assert.That(allVersions.Count(), Is.EqualTo(1));
            Assert.That(publishUtc, Is.Not.EqualTo(unpublishUtc));
            Assert.That(latestItem.PublishedUtc, Is.EqualTo(publishUtc));
            Assert.That(latestItem.VersionPublishedUtc, Is.EqualTo(publishUtc));
            Assert.That(latestItem.ContentItem.VersionRecord.Latest, Is.True);
            Assert.That(latestItem.ContentItem.VersionRecord.Published, Is.False);
        }
    }
}

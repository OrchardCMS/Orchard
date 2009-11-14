using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac.Builder;
using NUnit.Framework;
using Orchard.CmsPages.Models;
using Orchard.CmsPages.Services;
using Orchard.CmsPages.Services.Templates;
using Orchard.CmsPages.ViewModels;
using Orchard.Data;

namespace Orchard.Tests.Packages.Pages.Services {
    [TestFixture]
    public class PageManagerTests : DatabaseEnabledTestsBase {
        private IPageManager _manager;
        private ITemplateProvider _templateProvider;

        public override void Init() {
            base.Init();

            _manager = _container.Resolve<IPageManager>();
            _templateProvider = _container.Resolve<ITemplateProvider>();
        }

        public override void Register(ContainerBuilder builder) {
            builder.Register<PageManager>().As<IPageManager>();
            builder.Register(new StubTemplateProvider()).As<ITemplateProvider>();
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof (Page), typeof (PageRevision), typeof (ContentItem), typeof (Published),
                                 typeof (Scheduled)
                             };
            }
        }

        class StubTemplateProvider : ITemplateProvider {
            public IList<TemplateDescriptor> List() {
                return new List<TemplateDescriptor> {
                                                        new TemplateDescriptor { Name = "twocolumn", Zones = new[] { "content1", "content2" } }
                                                    };
            }

            public TemplateDescriptor Get(string name) {
                if (name == "twocolumn") {
                    return List()[0];
                }
                return null;
            }
        }


        [Test]
        public void CreatePageShouldAddInitialRecordsIncludingEmptyContentZonesFromTemplate() {
            var page = _manager.CreatePage(PageCreate("foo", "The Foo Page", "twocolumn"));

            Assert.That(page.Slug, Is.EqualTo("foo"));
            Assert.That(page.Title, Is.EqualTo("The Foo Page"));
            Assert.That(page.TemplateName, Is.EqualTo("twocolumn"));
            Assert.That(page.Contents, Has.Count.EqualTo(2));
            Assert.That(page.Contents, Has.Some.Property("ZoneName").EqualTo("content1"));
            Assert.That(page.Contents, Has.Some.Property("ZoneName").EqualTo("content2"));
        }

        private PageCreateViewModel PageCreate(string slug, string title, string template) {
            return new PageCreateViewModel { Slug = slug, Title = title, TemplateName = template, Templates = _templateProvider.List() };
        }

        [Test]
        public void CreatePageWithNullEmptyOrInvalidTemplateNameStillCreatesPage() {
            var page1 = _manager.CreatePage(PageCreate("foo1", "The Foo1 Page", null));
            var page2 = _manager.CreatePage(PageCreate("foo2", "The Foo2 Page", string.Empty));
            var page3 = _manager.CreatePage(PageCreate("foo3", "The Foo3 Page", "NoSuchTemplate"));

            Assert.That(page1.Id, Is.Not.EqualTo(0));
            Assert.That(page2.Id, Is.Not.EqualTo(0));
            Assert.That(page3.Id, Is.Not.EqualTo(0));

            //TODO: should a default named "content" always be assumed?
            Assert.That(page1.Contents, Has.Count.EqualTo(0));
            Assert.That(page2.Contents, Has.Count.EqualTo(0));
            Assert.That(page3.Contents, Has.Count.EqualTo(0));
        }

        [Test]
        public void ContentItemRecordsShouldFlushCorrectlyOnPageCreate() {
            var page1 = _manager.CreatePage(PageCreate("foo", "The Foo Page", "twocolumn"));
            _session.Clear();
            var page2 = _manager.GetLastRevision(page1.Id);
            Assert.That(page2.Contents, Has.Count.EqualTo(2));
        }

        [Test]
        public void GetPublishedBySlugShouldOnlyReturnPageAfterItIsPublished() {
            var revision = _manager.CreatePage(new PageCreateViewModel { Slug = "hello-world", Templates = _templateProvider.List() });

            var notPublishedYet = _manager.GetPublishedBySlug("hello-world");
            Assert.That(notPublishedYet, Is.Null);

            _manager.Publish(revision, new PublishOptions());

            var publishedNow = _manager.GetPublishedBySlug("hello-world");
            Assert.That(publishedNow, Is.Not.Null);
        }

        [Test]
        [Ignore("Linq to NHib doesn't support calling 'String.Equals' in expressions. Figure out a workaround.")]
        public void GetPublishedBySlugShouldBeCaseInsensitive() {
            var revision = _manager.CreatePage(new PageCreateViewModel { Slug = "hello-world", Templates = _templateProvider.List() });
            _manager.Publish(revision, new PublishOptions());

            var publishedNow = _manager.GetPublishedBySlug("hello-WORLD");
            Assert.That(publishedNow, Is.Not.Null);
        }

        [Test]
        public void PublishingPagesAddsToCurrentlyPublishedSlugList() {
            _manager.Publish(_manager.CreatePage(new PageCreateViewModel { Slug = "one", Templates = _templateProvider.List() }), new PublishOptions());
            _manager.Publish(_manager.CreatePage(new PageCreateViewModel { Slug = "two", Templates = _templateProvider.List() }), new PublishOptions());
            _manager.Publish(_manager.CreatePage(new PageCreateViewModel { Slug = "three", Templates = _templateProvider.List() }), new PublishOptions());

            var slugs = _manager.GetCurrentlyPublishedSlugs();
            Assert.That(slugs, Has.Count.GreaterThanOrEqualTo(3));
            Assert.That(slugs, Has.Some.EqualTo("one"));
            Assert.That(slugs, Has.Some.EqualTo("two"));
            Assert.That(slugs, Has.Some.EqualTo("three"));
        }

        [Test]
        public void PublishingPagesDoesNotChangeSlugCasing() {
            _manager.Publish(_manager.CreatePage(new PageCreateViewModel { Slug = "One", Templates = _templateProvider.List() }), new PublishOptions());
            _manager.Publish(_manager.CreatePage(new PageCreateViewModel { Slug = "TWO", Templates = _templateProvider.List() }), new PublishOptions());
            _manager.Publish(_manager.CreatePage(new PageCreateViewModel { Slug = "thRee", Templates = _templateProvider.List() }), new PublishOptions());

            var slugs = _manager.GetCurrentlyPublishedSlugs();
            Assert.That(slugs, Has.Count.GreaterThanOrEqualTo(3));
            Assert.That(slugs, Has.Some.EqualTo("One"));
            Assert.That(slugs, Has.Some.EqualTo("TWO"));
            Assert.That(slugs, Has.Some.EqualTo("thRee"));
        }

        [Test]
        public void PublishingThePublishedRevisionDoesNothing() {
            var initial = _manager.CreatePage(new PageCreateViewModel { Slug = "foo", Templates = _templateProvider.List() });
            _manager.Publish(initial, new PublishOptions());

            DateTime initialRevisionTime = initial.PublishedDate.Value;

            _clock.Advance(TimeSpan.FromSeconds(1));

            _manager.Publish(initial, new PublishOptions());

            Assert.That(initial.Page.Revisions.Count, Is.EqualTo(1));
            Assert.That(initial.Page.Revisions[0], Is.SameAs(initial));
            Assert.That(initial.Number, Is.EqualTo(1));
            Assert.That(initial.PublishedDate, Is.EqualTo(initialRevisionTime));
        }

        [Test]
        public void AcquireDraftOnUnpublishedPageShouldReturnExistingRevision() {
            var initial = _manager.CreatePage(new PageCreateViewModel { Slug = "foo", Templates = _templateProvider.List() });
            var draft = _manager.AcquireDraft(initial.Page.Id);
            Assert.That(initial, Is.SameAs(draft));
        }

        [Test]
        public void AcquireDraftForUpdateOnPublishedPageShouldCreateNewRevision() {
            var initial = _manager.CreatePage(new PageCreateViewModel { Slug = "foo", Templates = _templateProvider.List() });
            _manager.Publish(initial, new PublishOptions());
            var draft = _manager.AcquireDraft(initial.Page.Id);
            Assert.That(initial, Is.Not.SameAs(draft));
            Assert.That(initial.Number, Is.LessThan(draft.Number));
        }

        [Test]
        public void PublishingDraftWithKeepHistoryFalseShouldDeletePreviousPublishedRevision() {
            var initial = _manager.CreatePage(new PageCreateViewModel { Slug = "foo", Templates = _templateProvider.List() });
            var pageId = initial.Page.Id;

            _manager.Publish(initial, new PublishOptions());
            var draft = _manager.AcquireDraft(pageId);
            _manager.Publish(draft, new PublishOptions { History = PublishHistory.Discard });

            _session.Flush();
            _session.Clear();

            var lastRevision = _manager.GetLastRevision(pageId);


            Assert.That(lastRevision.Id, Is.EqualTo(draft.Id));
            Assert.That(lastRevision.Page.Revisions, Has.Count.EqualTo(1));
        }

        [Test]
        public void PublishingDraftWithKeepHistoryTrueShouldLeavePreviousRevisionIntact() {
            var initial = _manager.CreatePage(new PageCreateViewModel { Slug = "foo", Templates = _templateProvider.List() });
            var pageId = initial.Page.Id;

            Trace.WriteLine("Publish initial");
            _manager.Publish(initial, new PublishOptions());
            Trace.WriteLine("AcquireDraft");
            var draft = _manager.AcquireDraft(pageId);
            Trace.WriteLine("Publish draft");
            _manager.Publish(draft, new PublishOptions { History = PublishHistory.Preserve });

            _session.Flush();
            _session.Clear();

            Trace.WriteLine("GetLastRevision");
            var lastRevision = _manager.GetLastRevision(pageId);

            Assert.That(lastRevision.Id, Is.EqualTo(draft.Id));
            Assert.That(lastRevision.Page.Revisions, Has.Count.EqualTo(2));
        }

        [Test]
        public void PublishDateIsSetWhenPublishOccurs() {
            var initial = _manager.CreatePage(new PageCreateViewModel { Slug = "foo", Templates = _templateProvider.List() });
            Assert.That(initial.PublishedDate, Is.Null);
            _manager.Publish(initial, new PublishOptions());
            Assert.That(initial.PublishedDate, Is.EqualTo(_clock.UtcNow));
        }

        [Test]
        public void ModifiedDateIsSetWhenPageIsCreatedAndWhenAcquireDraftIsCalled() {
            var mark1 = _clock.UtcNow;

            var initial = _manager.CreatePage(new PageCreateViewModel { Slug = "foo", Templates = _templateProvider.List() });
            Assert.That(initial.PublishedDate, Is.Null);
            Assert.That(initial.ModifiedDate, Is.EqualTo(mark1));

            _clock.Advance(TimeSpan.FromMinutes(5));
            var mark2 = _clock.UtcNow;

            _manager.Publish(initial, new PublishOptions());
            Assert.That(initial.PublishedDate, Is.EqualTo(mark2));
            Assert.That(initial.ModifiedDate, Is.EqualTo(mark1));

            _clock.Advance(TimeSpan.FromMinutes(5));
            var mark3 = _clock.UtcNow;

            var draft = _manager.AcquireDraft(initial.Page.Id);
            Assert.That(draft.Id, Is.Not.EqualTo(initial.Id));
            Assert.That(draft.PublishedDate, Is.Null);
            Assert.That(draft.ModifiedDate, Is.EqualTo(mark3));

            Assert.That(mark1, Is.LessThan(mark2));
            Assert.That(mark2, Is.LessThan(mark3));

            // verify changes flushed as expected
            _session.Flush();
            _session.Clear();

            var reloaded = _manager.GetLastRevision(initial.Page.Id);

            Assert.That(reloaded.PublishedDate, Is.Null);
            Assert.That(reloaded.ModifiedDate, Is.EqualTo(mark3));

            Assert.That(reloaded.Page.Revisions, Has.Count.EqualTo(2));

            Assert.That(reloaded.Page.Revisions.First().PublishedDate, Is.EqualTo(mark2));
            Assert.That(reloaded.Page.Revisions.First().ModifiedDate, Is.EqualTo(mark1));
        }

        [Test]
        public void PublishedPropertyShouldCascadeInsertsAndDeletesWhenSetAndNulled() {
            var page = new Page {Published = new Published()};
            page.Published.Page = page;

            var pageRepos = _container.Resolve<IRepository<Page>>();
            pageRepos.Create(page);

            ClearSession();

            var page2 = pageRepos.Get(page.Id);

            Assert.That(page2.Published, Is.Not.Null);

            _container.Resolve<IRepository<Published>>().Delete(page2.Published);
            page2.Published = null;

            ClearSession();

            var page3 = pageRepos.Get(page.Id);
            Assert.That(page3.Published, Is.Null);
        }

        // more tests: 
        // publish new revision on same slug
        // publish on slug already published on different page
    }
}
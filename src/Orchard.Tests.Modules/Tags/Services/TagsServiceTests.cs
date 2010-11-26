using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Security;
using Orchard.Tags.Handlers;
using Orchard.Tags.Models;
using Orchard.Tags.Services;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;
using Orchard.UI.Notify;

namespace Orchard.Tests.Modules.Tags.Services {
    [TestFixture]
    public class TagsServiceTests : DatabaseEnabledTestsBase {
        private Mock<IAuthorizationService> _authz;
        private ITagService _tagService;
        private IContentManager _contentManager;

        public override void Register(ContainerBuilder builder) {
            _authz = new Mock<IAuthorizationService>();

            builder.RegisterAutoMocking(MockBehavior.Loose);
            builder.RegisterInstance(_authz.Object).As<IAuthorizationService>();
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<TagService>().As<ITagService>();
            builder.RegisterType<Notifier>().As<INotifier>();
            builder.RegisterType<ThingHandler>().As<IContentHandler>();
            builder.RegisterType<TagsPartHandler>().As<IContentHandler>();
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
        }

        public override void Init() {
            base.Init();

            _tagService = _container.Resolve<ITagService>();
            _contentManager = _container.Resolve<IContentManager>();
        }


        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                    typeof(ContentItemRecord), 
                    typeof(ContentItemVersionRecord), 
                    typeof(ContentTypeRecord), 
                    typeof(TagsPartRecord), 
                    typeof(TagRecord), 
                    typeof(ContentTagRecord)
                };
            }
        }

        [Test]
        public void TagServiceShouldResolve() {
        }

        [Test]
        public void CreateTagShouldBePersistent() {
            for (int i = 0; i < 10; i++) {
                _tagService.CreateTag("tag" + i);
            }

            ClearSession();

            var tag5 = _tagService.GetTagByName("tag5");
            Assert.That(tag5, Is.Not.Null);
            Assert.That(tag5.TagName, Is.EqualTo("tag5"));

            var tag50 = _tagService.GetTagByName("tag50");
            Assert.That(tag50, Is.Null);
        }


        [Test]
        public void TagsShouldBeAvailableWhenGettingContentItem() {
            var thing = _contentManager.New("thing");
            _contentManager.Create(thing);
            _tagService.UpdateTagsForContentItem(thing, new string[] { "tag1", "tag2", "tag3" });

            ClearSession();

            var thing2 = _contentManager.Get(thing.Id);
            Assert.That(thing2.As<TagsPart>().CurrentTags.Any(tagRecord => tagRecord.TagName == "tag1"), Is.True);
            Assert.That(thing2.As<TagsPart>().CurrentTags.Any(tagRecord => tagRecord.TagName == "tag2"), Is.True);
            Assert.That(thing2.As<TagsPart>().CurrentTags.Any(tagRecord => tagRecord.TagName == "tag3"), Is.True);
        }

        [Test]
        public void TagsShouldDeletedAferRemovingContentItem() {
            var thing = _contentManager.New("thing");
            _contentManager.Create(thing, VersionOptions.Published);
            _tagService.UpdateTagsForContentItem(thing, new string[] { "tag1", "tag2", "tag3" });

            ClearSession();

            Assert.That(_tagService.GetTagByName("tag1"), Is.Not.Null);
            Assert.That(_tagService.GetTagByName("tag2"), Is.Not.Null);
            Assert.That(_tagService.GetTagByName("tag3"), Is.Not.Null);

            _contentManager.Remove(_contentManager.Get(thing.Id));

            ClearSession();

            Assert.That(_tagService.GetTagByName("tag1"), Is.Null);
            Assert.That(_tagService.GetTagByName("tag2"), Is.Null);
            Assert.That(_tagService.GetTagByName("tag3"), Is.Null);
        }

        [Test]
        public void ContentItemsShouldBeReturnedFromTagService() {
            var thing1 = _contentManager.New("thing");
            _contentManager.Create(thing1);
            _tagService.UpdateTagsForContentItem(thing1, new string[] { "tag1", "tag2", "tag3" });

            var thing2 = _contentManager.New("thing");
            _contentManager.Create(thing2);
            _tagService.UpdateTagsForContentItem(thing2, new string[] { "tag4", "tag3" });

            ClearSession();

            Assert.That(_tagService.GetTaggedContentItems(25), Is.Empty);

            Assert.That(_tagService.GetTaggedContentItems(_tagService.GetTagByName("tag1").Id).Count(), Is.EqualTo(1));
            Assert.That(_tagService.GetTaggedContentItems(_tagService.GetTagByName("tag1").Id).Any(c => c.Id == thing1.Id), Is.True);

            Assert.That(_tagService.GetTaggedContentItems(_tagService.GetTagByName("tag2").Id).Count(), Is.EqualTo(1));
            Assert.That(_tagService.GetTaggedContentItems(_tagService.GetTagByName("tag2").Id).Any(c => c.Id == thing1.Id), Is.True);

            Assert.That(_tagService.GetTaggedContentItems(_tagService.GetTagByName("tag3").Id).Count(), Is.EqualTo(2));
            Assert.That(_tagService.GetTaggedContentItems(_tagService.GetTagByName("tag3").Id).Any(c => c.Id == thing1.Id), Is.True);
            Assert.That(_tagService.GetTaggedContentItems(_tagService.GetTagByName("tag3").Id).Any(c => c.Id == thing2.Id), Is.True);

            Assert.That(_tagService.GetTaggedContentItems(_tagService.GetTagByName("tag4").Id).Count(), Is.EqualTo(1));
            Assert.That(_tagService.GetTaggedContentItems(_tagService.GetTagByName("tag4").Id).Any(c => c.Id == thing2.Id), Is.True);
        }

        [Test]
        public void TagsDeletionShouldDeleteTagsAndAssociations() {
            var thing1 = _contentManager.New("thing");
            _contentManager.Create(thing1);
            _tagService.UpdateTagsForContentItem(thing1, new string[] {"tag1", "tag2", "tag3"});

            var thing2 = _contentManager.New("thing");
            _contentManager.Create(thing2);
            _tagService.UpdateTagsForContentItem(thing2, new string[] {"tag2", "tag3", "tag4"});

            ClearSession();

            Trace.WriteLine(string.Format("Delete tag \"{0}\"", "tag1"));
            _tagService.DeleteTag(_tagService.GetTagByName("tag1").Id);
            ClearSession();

            Assert.That(_tagService.GetTagByName("tag1"), Is.Null);

            var test = _contentManager.Get(thing1.Id);
            Assert.That(test.As<TagsPart>().Record.Tags.Count, Is.EqualTo(2));
            Assert.That(test.As<TagsPart>().Record.Tags.Any(t => t.TagRecord.TagName == "tag2"), Is.True);
            Assert.That(test.As<TagsPart>().Record.Tags.Any(t => t.TagRecord.TagName == "tag3"), Is.True);


            Trace.WriteLine(string.Format("Delete tag \"{0}\"", "tag2"));
            _tagService.DeleteTag(_tagService.GetTagByName("tag2").Id);
            ClearSession();

            Assert.That(_tagService.GetTagByName("tag2"), Is.Null);

            test = _contentManager.Get(thing1.Id);
            Assert.That(test.As<TagsPart>().Record.Tags.Count, Is.EqualTo(1));
            Assert.That(test.As<TagsPart>().Record.Tags.Any(t => t.TagRecord.TagName == "tag3"), Is.True);

            test = _contentManager.Get(thing2.Id);
            Assert.That(test.As<TagsPart>().Record.Tags.Count, Is.EqualTo(2));
            Assert.That(test.As<TagsPart>().Record.Tags.Any(t => t.TagRecord.TagName == "tag3"), Is.True);
            Assert.That(test.As<TagsPart>().Record.Tags.Any(t => t.TagRecord.TagName == "tag4"), Is.True);

            Trace.WriteLine(string.Format("Delete tag \"{0}\"", "tag3"));
            _tagService.DeleteTag(_tagService.GetTagByName("tag3").Id);

            Trace.WriteLine(string.Format("Delete tag \"{0}\"", "tag4"));
            _tagService.DeleteTag(_tagService.GetTagByName("tag4").Id);
            ClearSession();

            Assert.That(_tagService.GetTags(), Is.Empty);
        }


        [Test]
        public void TagsAssociationsShouldBeCreatedCorrectly() {
            var thing1 = _contentManager.New("thing");
            _contentManager.Create(thing1);
            _tagService.UpdateTagsForContentItem(thing1, new string[] { "tag1", "tag2", "tag3" });

            var thing2 = _contentManager.New("thing");
            _contentManager.Create(thing2);
            _tagService.UpdateTagsForContentItem(thing2, new string[] { "tag2", "tag3", "tag4" });

            ClearSession();

            var test = _contentManager.Get(thing1.Id);
            Assert.That(test.As<TagsPart>().Record.Tags.Count, Is.EqualTo(3));
            Assert.That(test.As<TagsPart>().Record.Tags.Any(t => t.TagRecord.TagName == "tag1"), Is.True);
            Assert.That(test.As<TagsPart>().Record.Tags.Any(t => t.TagRecord.TagName == "tag2"), Is.True);
            Assert.That(test.As<TagsPart>().Record.Tags.Any(t => t.TagRecord.TagName == "tag3"), Is.True);

            test = _contentManager.Get(thing2.Id);
            Assert.That(test.As<TagsPart>().Record.Tags.Count, Is.EqualTo(3));
            Assert.That(test.As<TagsPart>().Record.Tags.Any(t => t.TagRecord.TagName == "tag2"), Is.True);
            Assert.That(test.As<TagsPart>().Record.Tags.Any(t => t.TagRecord.TagName == "tag3"), Is.True);
            Assert.That(test.As<TagsPart>().Record.Tags.Any(t => t.TagRecord.TagName == "tag4"), Is.True);

            Assert.That(_tagService.GetTagByName("tag1").ContentTags.Count, Is.EqualTo(1));
            Assert.That(_tagService.GetTagByName("tag1").ContentTags.Any(t => t.TagsPartRecord.Id == thing1.Id), Is.True);

            Assert.That(_tagService.GetTagByName("tag2").ContentTags.Count, Is.EqualTo(2));
            Assert.That(_tagService.GetTagByName("tag2").ContentTags.Any(t => t.TagsPartRecord.Id == thing1.Id), Is.True);
            Assert.That(_tagService.GetTagByName("tag2").ContentTags.Any(t => t.TagsPartRecord.Id == thing2.Id), Is.True);

            Assert.That(_tagService.GetTagByName("tag3").ContentTags.Count, Is.EqualTo(2));
            Assert.That(_tagService.GetTagByName("tag3").ContentTags.Any(t => t.TagsPartRecord.Id == thing1.Id), Is.True);
            Assert.That(_tagService.GetTagByName("tag3").ContentTags.Any(t => t.TagsPartRecord.Id == thing2.Id), Is.True);

            Assert.That(_tagService.GetTagByName("tag4").ContentTags.Count, Is.EqualTo(1));
            Assert.That(_tagService.GetTagByName("tag4").ContentTags.Any(t => t.TagsPartRecord.Id == thing2.Id), Is.True);
        }

        [Test]
        public void RenamingATagShouldMergeTaggedItems() {
            var thing1 = _contentManager.New("thing");
            _contentManager.Create(thing1);
            _tagService.UpdateTagsForContentItem(thing1, new string[] { "tag1", "tag2", "tag3" });

            var thing2 = _contentManager.New("thing");
            _contentManager.Create(thing2);
            _tagService.UpdateTagsForContentItem(thing2, new string[] { "tag2" });

            var thing3 = _contentManager.New("thing");
            _contentManager.Create(thing3);
            _tagService.UpdateTagsForContentItem(thing3, new string[] { "tag3" });

            // Renamed and merge "tag2" to "tag3"
            var tag = _tagService.GetTagByName("tag2");
            _tagService.UpdateTag(tag.Id, "tag3");

            ClearSession();

            Assert.That(_tagService.GetTags().Count(), Is.EqualTo(2));
            Assert.That(_tagService.GetTags().Any(tagRecord => tagRecord.TagName == "tag1"), Is.True);
            Assert.That(_tagService.GetTags().Any(tagRecord => tagRecord.TagName == "tag2"), Is.False);
            Assert.That(_tagService.GetTags().Any(tagRecord => tagRecord.TagName == "tag3"), Is.True);

            Assert.That(_contentManager.Get(thing1.Id).As<TagsPart>().CurrentTags.Any(tagRecord => tagRecord.TagName == "tag1"), Is.True);
            Assert.That(_contentManager.Get(thing1.Id).As<TagsPart>().CurrentTags.Any(tagRecord => tagRecord.TagName == "tag2"), Is.False);
            Assert.That(_contentManager.Get(thing1.Id).As<TagsPart>().CurrentTags.Any(tagRecord => tagRecord.TagName == "tag3"), Is.True);

            Assert.That(_contentManager.Get(thing2.Id).As<TagsPart>().CurrentTags.Any(tagRecord => tagRecord.TagName == "tag3"), Is.True);

            Assert.That(_contentManager.Get(thing3.Id).As<TagsPart>().CurrentTags.Any(tagRecord => tagRecord.TagName == "tag3"), Is.True);
        }

        public class ThingHandler : ContentHandler {
            public ThingHandler() {
                Filters.Add(new ActivatingFilter<Thing>("thing"));
                Filters.Add(new ActivatingFilter<TagsPart>("thing"));
            }
        }

        public class Thing : ContentPart {
        }
    }
}

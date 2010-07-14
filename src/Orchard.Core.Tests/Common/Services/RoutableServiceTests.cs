using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable;
using Orchard.Core.Routable.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Tests.Modules;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Tests.Stubs;

namespace Orchard.Core.Tests.Common.Services {
    [TestFixture]
    public class RoutableServiceTests : DatabaseEnabledTestsBase {
        [SetUp]
        public override void Init() {
            base.Init();
            _routableService = _container.Resolve<IRoutableService>();
        }

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IContentDefinitionManager>().Object);

            builder.RegisterType<ThingHandler>().As<IContentHandler>();
            builder.RegisterType<StuffHandler>().As<IContentHandler>();
            builder.RegisterType<RoutableService>().As<IRoutableService>();
            builder.RegisterType<RoutablePathConstraint>().As<IRoutablePathConstraint>();

            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();
            builder.RegisterInstance(new UrlHelper(new RequestContext(new StubHttpContext("~/"), new RouteData()))).As<UrlHelper>();
            builder.RegisterType<RoutableHandler>().As<IContentHandler>();

        }

        private IRoutableService _routableService;

        [Test]
        public void InvalidCharactersShouldBeReplacedByADash() {
            var contentManager = _container.Resolve<IContentManager>();

            var thing = contentManager.Create<Thing>(ThingDriver.ContentType.Name, t => {
                t.As<IsRoutable>().Record = new RoutableRecord();
                t.Title = "Please do not use any of the following characters in your slugs: \":\", \"/\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\"";
            });

            _routableService.FillSlug(thing.As<IsRoutable>());

            Assert.That(thing.Slug, Is.EqualTo("please-do-not-use-any-of-the-following-characters-in-your-slugs-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\""));
        }

        [Test]
        public void EmptySlugsShouldBeConsideredValid() {
            // so that automatic generation on Publish occurs
            Assert.That(_routableService.IsSlugValid(null), Is.True);
            Assert.That(_routableService.IsSlugValid(String.Empty), Is.True);
            Assert.That(_routableService.IsSlugValid("    "), Is.True);
        }

        [Test]
        public void InvalidCharacterShouldBeRefusedInSlugs() {
            Assert.That(_routableService.IsSlugValid("aaaa-_aaaa"), Is.True);

            foreach (var c in @"/:?#[]@!$&'()*+,;= ") {
                Assert.That(_routableService.IsSlugValid("a" + c + "b"), Is.False);
            }
        }


        [Test]
        public void VeryLongStringTruncatedTo1000Chars() {
            var contentManager = _container.Resolve<IContentManager>();

            var veryVeryLongTitle = "this is a very long title...";
            for (var i = 0; i < 100; i++)
                veryVeryLongTitle += "aaaaaaaaaa";

            var thing = contentManager.Create<Thing>(ThingDriver.ContentType.Name, t => {
                t.As<IsRoutable>().Record = new RoutableRecord();
                t.Title = veryVeryLongTitle;
            });

            _routableService.FillSlug(thing.As<IsRoutable>());

            Assert.That(veryVeryLongTitle.Length, Is.AtLeast(1001));
            Assert.That(thing.Slug.Length, Is.EqualTo(1000));
        }

        [Test]
        public void NoExistingLikeSlugsGeneratesSameSlug() {
            string slug = _routableService.GenerateUniqueSlug("woohoo", null);
            Assert.That(slug, Is.EqualTo("woohoo"));
        }

        [Test]
        public void ExistingSingleLikeSlugThatsAConflictGeneratesADash2() {
            string slug = _routableService.GenerateUniqueSlug("woohoo", new List<string> { "woohoo" });
            Assert.That(slug, Is.EqualTo("woohoo-2"));
        }

        [Test]
        public void ExistingSingleLikeSlugThatsNotAConflictGeneratesSameSlug() {
            string slug = _routableService.GenerateUniqueSlug("woohoo", new List<string> { "woohoo-2" });
            Assert.That(slug, Is.EqualTo("woohoo"));
        }

        [Test]
        public void ExistingLikeSlugsWithAConflictGeneratesADashVNext() {
            string slug = _routableService.GenerateUniqueSlug("woohoo", new List<string> { "woohoo", "woohoo-2" });
            Assert.That(slug, Is.EqualTo("woohoo-3"));
        }

        [Test]
        public void ExistingSlugsWithVersionGapsAndNoMatchGeneratesSameSlug() {
            string slug = _routableService.GenerateUniqueSlug("woohoo", new List<string> { "woohoo-2", "woohoo-4", "woohoo-5" });
            Assert.That(slug, Is.EqualTo("woohoo"));
        }

        [Test]
        public void ExistingSlugsWithVersionGapsAndAMatchGeneratesADash2() {
            string slug = _routableService.GenerateUniqueSlug("woohoo-2", new List<string> { "woohoo-2", "woohoo-4", "woohoo-5" });
            Assert.That(slug, Is.EqualTo("woohoo-2-2"));
        }

        [Test]
        public void GeneratedSlugIsLowerCased() {
            var contentManager = _container.Resolve<IContentManager>();

            var thing = contentManager.Create<Thing>(ThingDriver.ContentType.Name, t => {
                t.As<IsRoutable>().Record = new RoutableRecord();
                t.Title = "This Is Some Interesting Title";
            });

            _routableService.FillSlug(thing.As<IsRoutable>());

            Assert.That(thing.Slug, Is.EqualTo("this-is-some-interesting-title"));
        }

        [Test]
        public void GeneratedSlugsShouldBeUniqueAmongContentType() {
            var contentManager = _container.Resolve<IContentManager>();

            var thing1 = contentManager.Create<Thing>(ThingDriver.ContentType.Name, t => {
                t.As<IsRoutable>().Record = new RoutableRecord();
                t.Title = "This Is Some Interesting Title";
            });

            var thing2 = contentManager.Create<Thing>(ThingDriver.ContentType.Name, t => {
                t.As<IsRoutable>().Record = new RoutableRecord();
                t.Title = "This Is Some Interesting Title";
            });

            Assert.AreNotEqual(thing1.Slug, thing2.Slug);
        }

        [Test]
        public void SlugsCanBeDuplicatedAccrossContentTypes() {
            var contentManager = _container.Resolve<IContentManager>();

            var thing = contentManager.Create<Thing>(ThingDriver.ContentType.Name, t => {
                t.As<IsRoutable>().Record = new RoutableRecord();
                t.Title = "This Is Some Interesting Title";
            });

            var stuff = contentManager.Create<Stuff>(StuffDriver.ContentType.Name, s => {
                s.As<IsRoutable>().Record = new RoutableRecord();
                s.Title = "This Is Some Interesting Title";
            });

            Assert.AreEqual(thing.Slug, stuff.Slug);
        }


        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof(RoutableRecord), 
                                 typeof(ContentTypeRecord),
                                 typeof(ContentItemRecord), 
                                 typeof(ContentItemVersionRecord), 
                                 typeof(CommonRecord),
                                 typeof(CommonVersionRecord),
                             };
            }
        }

        [UsedImplicitly]
        public class ThingHandler : ContentHandler {
            public ThingHandler() {
                Filters.Add(new ActivatingFilter<Thing>(ThingDriver.ContentType.Name));
                Filters.Add(new ActivatingFilter<ContentPart<CommonVersionRecord>>(ThingDriver.ContentType.Name));
                Filters.Add(new ActivatingFilter<CommonAspect>(ThingDriver.ContentType.Name));
                Filters.Add(new ActivatingFilter<IsRoutable>(ThingDriver.ContentType.Name));
            }
        }

        public class Thing : ContentPart {
            public int Id { get { return ContentItem.Id; } }

            public string Title {
                get { return this.As<IsRoutable>().Title; }
                set { this.As<IsRoutable>().Title = value; }
            }

            public string Slug {
                get { return this.As<IsRoutable>().Slug; }
                set { this.As<IsRoutable>().Slug = value; }
            }
        }

        public class ThingDriver : ContentItemDriver<Thing> {
            public readonly static ContentType ContentType = new ContentType {
                Name = "thing",
                DisplayName = "Thing"
            };
        }

        [UsedImplicitly]
        public class StuffHandler : ContentHandler {
            public StuffHandler() {
                Filters.Add(new ActivatingFilter<Stuff>(StuffDriver.ContentType.Name));
                Filters.Add(new ActivatingFilter<ContentPart<CommonVersionRecord>>(StuffDriver.ContentType.Name));
                Filters.Add(new ActivatingFilter<CommonAspect>(StuffDriver.ContentType.Name));
                Filters.Add(new ActivatingFilter<IsRoutable>(StuffDriver.ContentType.Name));
            }
        }

        public class Stuff : ContentPart {
            public int Id { get { return ContentItem.Id; } }

            public string Title {
                get { return this.As<IsRoutable>().Title; }
                set { this.As<IsRoutable>().Title = value; }
            }

            public string Slug {
                get { return this.As<IsRoutable>().Slug; }
                set { this.As<IsRoutable>().Slug = value; }
            }
        }

        public class StuffDriver : ContentItemDriver<Stuff> {
            public readonly static ContentType ContentType = new ContentType {
                Name = "stuff",
                DisplayName = "Stuff"
            };
        }
    }
}
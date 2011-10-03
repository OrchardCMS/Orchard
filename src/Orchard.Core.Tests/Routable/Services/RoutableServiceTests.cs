using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable;
using Orchard.Core.Routable.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Tests.Modules;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Tests.Stubs;
using Orchard.UI.Notify;

namespace Orchard.Core.Tests.Routable.Services {
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
            builder.RegisterInstance(new Mock<ITransactionManager>().Object);
            builder.RegisterInstance(new Mock<IAuthorizer>().Object);
            builder.RegisterInstance(new Mock<INotifier>().Object);
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);
            builder.RegisterType<StubHttpContextAccessor>().As<IHttpContextAccessor>();
            builder.RegisterType<WorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();

            builder.RegisterType<ThingHandler>().As<IContentHandler>();
            builder.RegisterType<StuffHandler>().As<IContentHandler>();
            builder.RegisterType<RoutableService>().As<IRoutableService>();
            builder.RegisterType<RoutablePathConstraint>().As<IRoutablePathConstraint>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<ShapeTableLocator>().As<IShapeTableLocator>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();

            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();
            builder.RegisterInstance(new UrlHelper(new RequestContext(new StubHttpContext("~/"), new RouteData()))).As<UrlHelper>();
            builder.RegisterType<RoutePartHandler>().As<IContentHandler>();

            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();
        }

        private IRoutableService _routableService;

        [Test]
        public void InvalidCharactersShouldBeReplacedByADash() {
            var contentManager = _container.Resolve<IContentManager>();

            var thing = contentManager.Create<Thing>("thing", t => {
                t.As<RoutePart>().Record = new RoutePartRecord();
                t.Title = "Please do not use any of the following characters in your permalink: \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\", \"\"\", \"<\", \">\", \"\\\"";
            });

            _routableService.FillSlugFromTitle(thing.As<RoutePart>());

            Assert.That(thing.Slug, Is.EqualTo("please-do-not-use-any-of-the-following-characters-in-your-permalink"));
        }

        [Test]
        public void SpacesSlugShouldBeTreatedAsEmpty() {
            var contentManager = _container.Resolve<IContentManager>();

            var thing = contentManager.Create<Thing>("thing", t => {
                t.As<RoutePart>().Record = new RoutePartRecord();
                t.Title = "My Title";
                t.Slug = " ";
            });

            _routableService.FillSlugFromTitle(thing.As<RoutePart>());

            Assert.That(thing.Slug, Is.EqualTo("my-title"));
        }


        [Test]
        public void SlashInSlugIsAllowed() {
            Assert.That(_routableService.IsSlugValid("some/page"), Is.True);
        }

        [Test]
        public void DotsAroundSlugAreAllowed() {
            Assert.That(_routableService.IsSlugValid(".slug"), Is.False);
            Assert.That(_routableService.IsSlugValid("slug."), Is.False);
            Assert.That(_routableService.IsSlugValid("slug.slug"), Is.True);
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

            foreach (var c in @":?#[]@!$&'()*+,;= \") {
                Assert.That(_routableService.IsSlugValid("a" + c + "b"), Is.False);
            }
        }

        [Test]
        public void VeryLongStringTruncatedTo1000Chars() {
            var veryVeryLongTitle = "this is a very long title...";
            for (var i = 0; i < 100; i++)
                veryVeryLongTitle += "aaaaaaaaaa";

            var thing = CreateRoutePartFromScratch(veryVeryLongTitle);
            _routableService.FillSlugFromTitle(thing);

            Assert.That(veryVeryLongTitle.Length, Is.AtLeast(1001));
            Assert.That(thing.Slug.Length, Is.EqualTo(1000));
        }

        [Test]
        public void NoExistingLikeSlugsGeneratesSameSlug() {
            string slug = _routableService.GenerateUniqueSlug(CreateRoutePartFromScratch("woohoo"), null);
            Assert.That(slug, Is.EqualTo("woohoo"));
        }

        [Test]
        public void ExistingSingleLikeSlugThatsAConflictGeneratesADash2() {
            string slug = _routableService.GenerateUniqueSlug(CreateRoutePartFromScratch("woohoo"), new List<string> { "woohoo" });
            Assert.That(slug, Is.EqualTo("woohoo-2"));
        }

        [Test]
        public void ExistingSingleLikeSlugThatsNotAConflictGeneratesSameSlug() {
            string slug = _routableService.GenerateUniqueSlug(CreateRoutePartFromScratch("woohoo"), new List<string> { "woohoo-2" });
            Assert.That(slug, Is.EqualTo("woohoo"));
        }

        [Test]
        public void ExistingLikeSlugsWithAConflictGeneratesADashVNext() {
            string slug = _routableService.GenerateUniqueSlug(CreateRoutePartFromScratch("woohoo"), new List<string> { "woohoo", "woohoo-2" });
            Assert.That(slug, Is.EqualTo("woohoo-3"));
        }

        [Test]
        public void ExistingSlugsWithVersionGapsAndNoMatchGeneratesSameSlug() {
            string slug = _routableService.GenerateUniqueSlug(CreateRoutePartFromScratch("woohoo"), new List<string> { "woohoo-2", "woohoo-4", "woohoo-5" });
            Assert.That(slug, Is.EqualTo("woohoo"));
        }

        [Test]
        public void ExistingSlugsWithVersionGapsAndAMatchGeneratesADash2() {
            string slug = _routableService.GenerateUniqueSlug(CreateRoutePartFromScratch("woohoo-2"), new List<string> { "woohoo-2", "woohoo-4", "woohoo-5" });
            Assert.That(slug, Is.EqualTo("woohoo-2-2"));
        }

        [Test]
        public void SlugIsGeneratedLowerCased() {
            var thing = CreateRoutePartFromScratch("This Is Some Interesting Title");
            _routableService.FillSlugFromTitle(thing);
            Assert.That(thing.Slug, Is.EqualTo("this-is-some-interesting-title"));
        }

        [Test]
        public void SlugInConflictWithAnExistingItemsPathIsVersioned() {
            CreateRoutePartFromScratch("bar", "bar", "foo");
            var thing2 = CreateRoutePartFromScratch("fooslashbar", "foo/bar");
            Assert.That(thing2.Path, Is.EqualTo("foo/bar-2"));
        }

        [Test]
        public void GeneratedSlugInConflictInSameContaierPathIsVersioned() {
            var thing1 = CreateRoutePartFromScratch("Foo", "", "bar");
            var thing2 = CreateRoutePartWithExistingContainer("Foo", thing1.As<ICommonPart>().Container);
            Assert.That(thing2.Path, Is.EqualTo("bar/foo-2"));
            Assert.That(thing2.Slug, Is.EqualTo("foo-2"));
        }

        [Test]
        public void GivenSlugInConflictInSameContaierPathIsVersioned() {
            var thing1 = CreateRoutePartFromScratch("Hi", "foo", "bar");
            var thing2 = CreateRoutePartWithExistingContainer("There", thing1.As<ICommonPart>().Container, "foo");
            Assert.That(thing2.Path, Is.EqualTo("bar/foo-2"));
            Assert.That(thing2.Slug, Is.EqualTo("foo-2"));
        }

        [Test]
        public void GeneratedSlugInConflictInDifferentContaierPathIsNotVersioned() {
            var thing1 = CreateRoutePartFromScratch("Foo", "", "rab");
            var thing2 = CreateRoutePartFromScratch("Foo", "", "bar");
            Assert.That(thing1.Path, Is.EqualTo("rab/foo"));
            Assert.That(thing2.Path, Is.EqualTo("bar/foo"));
            Assert.That(thing1.Slug, Is.EqualTo("foo"));
            Assert.That(thing2.Slug, Is.EqualTo("foo"));
        }

        private RoutePart CreateRoutePartWithExistingContainer(string title, IContent container, string slug = "") {
            var contentManager = _container.Resolve<IContentManager>();
            return contentManager.Create<Thing>("thing", t => {
                t.As<RoutePart>().Record = new RoutePartRecord();
                t.Title = title;

                if (!string.IsNullOrWhiteSpace(slug))
                    t.As<RoutePart>().Slug = slug;

                if (container != null)
                    t.As<ICommonPart>().Container = container;
            }).As<RoutePart>();
        }

        private RoutePart CreateRoutePartFromScratch(string title, string slug = "", string containerPath = "") {
            var contentManager = _container.Resolve<IContentManager>();
            return contentManager.Create<Thing>("thing", t => {
                t.As<RoutePart>().Record = new RoutePartRecord();

                if (!string.IsNullOrWhiteSpace(slug))
                    t.As<RoutePart>().Slug = slug;

                t.Title = title;
                if (!string.IsNullOrWhiteSpace(containerPath)) {
                    t.As<ICommonPart>().Container = contentManager.Create<Thing>("thing", tt => {
                        tt.As<RoutePart>().Path = containerPath;
                        tt.As<RoutePart>().Slug = containerPath;
                        tt.As<RoutePart>().Title = "Test Container";
                    });
                }
            }).As<RoutePart>();
        }


        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof(RoutePartRecord), 
                                 typeof(ContentTypeRecord),
                                 typeof(ContentItemRecord), 
                                 typeof(ContentItemVersionRecord), 
                                 typeof(CommonPartRecord),
                                 typeof(CommonPartVersionRecord),
                             };
            }
        }

        [UsedImplicitly]
        public class ThingHandler : ContentHandler {
            public ThingHandler() {
                Filters.Add(new ActivatingFilter<Thing>("thing"));
                Filters.Add(new ActivatingFilter<ContentPart<CommonPartVersionRecord>>("thing"));
                Filters.Add(new ActivatingFilter<CommonPart>("thing"));
                Filters.Add(new ActivatingFilter<RoutePart>("thing"));
            }
        }

        public class Thing : ContentPart {
            public string Title {
                get { return this.As<RoutePart>().Title; }
                set { this.As<RoutePart>().Title = value; }
            }

            public string Slug {
                get { return this.As<RoutePart>().Slug; }
                set { this.As<RoutePart>().Slug = value; }
            }
        }

        [UsedImplicitly]
        public class StuffHandler : ContentHandler {
            public StuffHandler() {
                Filters.Add(new ActivatingFilter<Stuff>("stuff"));
                Filters.Add(new ActivatingFilter<ContentPart<CommonPartVersionRecord>>("stuff"));
                Filters.Add(new ActivatingFilter<CommonPart>("stuff"));
                Filters.Add(new ActivatingFilter<RoutePart>("stuff"));
            }
        }

        public class Stuff : ContentPart {
            public string Title {
                get { return this.As<RoutePart>().Title; }
                set { this.As<RoutePart>().Title = value; }
            }

            public string Slug {
                get { return this.As<RoutePart>().Slug; }
                set { this.As<RoutePart>().Slug = value; }
            }
        }
    }
}
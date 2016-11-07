using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Common.Models;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Controllers;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.Rss;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.Tests.Modules;
using Orchard.Tests.Stubs;
using Orchard.Core.Title.Models;

namespace Orchard.Core.Tests.Feeds.Controllers {
    [TestFixture]
    public class FeedControllerTests {
        [Test]
        public void InvalidFormatShpuldReturnNotFoundResult() {
            var controller = new FeedController(
                Enumerable.Empty<IFeedQueryProvider>(),
                Enumerable.Empty<IFeedBuilderProvider>(),
                new StubItemBuilder()
                ) {
                    ValueProvider = Values.From(new { })
                };

            var result = controller.Index("no-such-format");
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<HttpNotFoundResult>());
        }

        [Test]
        public void ControllerShouldReturnAnActionResult() {
            var formatProvider = new Mock<IFeedBuilderProvider>();
            var format = new Mock<IFeedBuilder>();
            formatProvider.Setup(x => x.Match(It.IsAny<FeedContext>()))
                .Returns(new FeedBuilderMatch { FeedBuilder = format.Object, Priority = 10 });

            var queryProvider = new Mock<IFeedQueryProvider>();
            var query = new Mock<IFeedQuery>();
            queryProvider.Setup(x => x.Match(It.IsAny<FeedContext>()))
                .Returns(new FeedQueryMatch { FeedQuery = query.Object, Priority = 10 });


            format.Setup(x => x.Process(It.IsAny<FeedContext>(), It.IsAny<Action>())).Returns(new ContentResult());

            var controller = new FeedController(
                new[] { queryProvider.Object },
                new[] { formatProvider.Object },
                new StubItemBuilder()
                ) {
                    ValueProvider = Values.From(new { })
                };

            var result = controller.Index("test-format");
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ActionResult>());

            formatProvider.Verify();
            queryProvider.Verify();
            format.Verify();
        }


        class StubQuery : IFeedQueryProvider, IFeedQuery {
            private readonly IEnumerable<ContentItem> _items;

            public StubQuery(IEnumerable<ContentItem> items) {
                _items = items;
            }

            public FeedQueryMatch Match(FeedContext context) {
                return new FeedQueryMatch { FeedQuery = this, Priority = 10 };
            }

            public void Execute(FeedContext context) {
                foreach (var item in _items) {
                    context.Builder.AddItem(context, item);
                }
            }
        }

        class StubItemBuilder : IFeedItemBuilder {
            public void Populate(FeedContext context) {
            }
        }

        [Test]
        public void RssFeedShouldBeStructuredAppropriately() {
            var query = new StubQuery(Enumerable.Empty<ContentItem>());

            var builder = new ContainerBuilder();
            builder.RegisterType<FeedController>();
            builder.RegisterType<RssFeedBuilder>().As<IFeedBuilderProvider>();
            builder.RegisterInstance(query).As<IFeedQueryProvider>();
            builder.RegisterInstance(new StubItemBuilder()).As<IFeedItemBuilder>();
            var container = builder.Build();

            var controller = container.Resolve<FeedController>();
            controller.ValueProvider = Values.From(new { });

            var result = controller.Index("rss");
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<RssResult>());

            var doc = ((RssResult)result).Document;
            Assert.That(doc.Root.Name, Is.EqualTo(XName.Get("rss")));
            Assert.That(doc.Root.Elements().Single().Name, Is.EqualTo(XName.Get("channel")));
        }

        [Test]
        public void OneItemPerContentItemShouldBeCreated() {
            var query = new StubQuery(new[] {
                new ContentItem(),
                new ContentItem(),
            });

            var builder = new ContainerBuilder();
            builder.RegisterType<FeedController>();
            builder.RegisterType<RssFeedBuilder>().As<IFeedBuilderProvider>();
            builder.RegisterInstance(new StubItemBuilder()).As<IFeedItemBuilder>();
            builder.RegisterInstance(query).As<IFeedQueryProvider>();
            var container = builder.Build();

            var controller = container.Resolve<FeedController>();
            controller.ValueProvider = Values.From(new { });

            var result = controller.Index("rss");
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<RssResult>());

            var doc = ((RssResult)result).Document;
            var items = doc.Elements("rss").Elements("channel").Elements("item");
            Assert.That(items.Count(), Is.EqualTo(2));
        }

        [Test]
        public void CorePartValuesAreExtracted() {
            var clock = new StubClock();
            var hello = new ContentItemBuilder(new ContentTypeDefinitionBuilder().Named("hello").Build())
                .Weld<CommonPart>()
                .Weld<TitlePart>()
                .Weld<BodyPart>()
                .Weld<InfosetPart>()
                .Build();
            hello.As<CommonPart>().Record = new CommonPartRecord();
            hello.As<TitlePart>().Record = new TitlePartRecord();
            hello.As<BodyPart>().Record = new BodyPartRecord();

            hello.As<CommonPart>().PublishedUtc = clock.UtcNow;
            hello.As<TitlePart>().Title = "alpha";
            // hello.As<RoutePart>().Slug = "beta";
            hello.As<BodyPart>().Text = "gamma";

            var query = new StubQuery(new[] {
                hello,
            });

            var mockContentManager = new Mock<IContentManager>();
            mockContentManager.Setup(x => x.GetItemMetadata(It.IsAny<IContent>()))
                .Returns(new ContentItemMetadata() { DisplayText = "foo" });

            var builder = new ContainerBuilder();
            //builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.RegisterType<FeedController>();
            builder.RegisterInstance(new RouteCollection());
            builder.RegisterInstance(mockContentManager.Object).As<IContentManager>();
            builder.RegisterType<RssFeedBuilder>().As<IFeedBuilderProvider>();
            builder.RegisterType<CorePartsFeedItemBuilder>().As<IFeedItemBuilder>();
            builder.RegisterInstance(query).As<IFeedQueryProvider>();
            var container = builder.Build();

            var controller = container.Resolve<FeedController>();
            controller.ValueProvider = Values.From(new { });

            var result = controller.Index("rss");
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<RssResult>());

            var doc = ((RssResult)result).Document;
            var item = doc.Elements("rss").Elements("channel").Elements("item").Single();
            Assert.That(item.Element("title").Value, Is.EqualTo("foo"));
            Assert.That(item.Element("description").Value, Is.EqualTo("gamma"));

        }
    }
}


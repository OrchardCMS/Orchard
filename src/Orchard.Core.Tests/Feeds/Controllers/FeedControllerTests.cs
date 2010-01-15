using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Autofac.Builder;
using Autofac.Modules;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Records;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Controllers;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.Rss;
using Orchard.Core.Feeds.Services;
using Orchard.Mvc.Results;
using Orchard.Tests.Packages;
using Orchard.Tests.Stubs;

namespace Orchard.Core.Tests.Feeds.Controllers {
    [TestFixture]
    public class FeedControllerTests {
        [Test]
        public void InvalidFormatShpuldReturnNotFoundResult() {
            var controller = new FeedController(
                Enumerable.Empty<IFeedQueryProvider>(),
                Enumerable.Empty<IFeedFormatterProvider>(),
                Enumerable.Empty<IFeedItemBuilder>()) {
                    ValueProvider = Values.From(new { })
                };

            var result = controller.Index("no-such-format");
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public void ControllerShouldReturnAnActionResult() {
            var formatProvider = new Mock<IFeedFormatterProvider>();
            var format = new Mock<IFeedFormatter>();
            formatProvider.Setup(x => x.Match(It.IsAny<FeedContext>()))
                .Returns(new FeedFormatterMatch { FeedFormatter = format.Object, Priority = 10 });

            var queryProvider = new Mock<IFeedQueryProvider>();
            var query = new Mock<IFeedQuery>();
            queryProvider.Setup(x => x.Match(It.IsAny<FeedContext>()))
                .Returns(new FeedQueryMatch { FeedQuery = query.Object, Priority = 10 });


            format.Setup(x => x.Process(It.IsAny<FeedContext>(), It.IsAny<Action>())).Returns(new ContentResult());

            var controller = new FeedController(
                new[] { queryProvider.Object },
                new[] { formatProvider.Object },
                Enumerable.Empty<IFeedItemBuilder>()) {
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
                    context.FeedFormatter.AddItem(context, item);
                }
            }
        }

        [Test]
        public void RssFeedShouldBeStructuredAppropriately() {
            var query = new StubQuery(Enumerable.Empty<ContentItem>());

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.Register<FeedController>();
            builder.Register<RssFeedFormatProvider>().As<IFeedFormatterProvider>();
            builder.Register(query).As<IFeedQueryProvider>();
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
            builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.Register<FeedController>();
            builder.Register<RssFeedFormatProvider>().As<IFeedFormatterProvider>();
            builder.Register(query).As<IFeedQueryProvider>();
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
            var hello = new ContentItemBuilder("hello")
                .Weld<CommonAspect>()
                .Weld<RoutableAspect>()
                .Weld<BodyAspect>()
                .Build();
            hello.As<CommonAspect>().Record = new CommonRecord();
            hello.As<RoutableAspect>().Record = new RoutableRecord();
            hello.As<BodyAspect>().Record = new BodyRecord();

            hello.As<CommonAspect>().PublishedUtc = clock.UtcNow;
            hello.As<RoutableAspect>().Title = "alpha";
            hello.As<RoutableAspect>().Slug = "beta";
            hello.As<BodyAspect>().Text = "gamma";

            var query = new StubQuery(new[] {
                hello,
            });

            var mockContentManager = new Mock<IContentManager>();
            mockContentManager.Setup(x => x.GetItemMetadata(It.IsAny<IContent>()))
                .Returns(new ContentItemMetadata { DisplayText = "foo" });

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.Register<FeedController>();
            builder.Register(new RouteCollection());
            builder.Register(mockContentManager.Object).As<IContentManager>();
            builder.Register<RssFeedFormatProvider>().As<IFeedFormatterProvider>();
            builder.Register<CorePartsFeedItemBuilder>().As<IFeedItemBuilder>();
            builder.Register(query).As<IFeedQueryProvider>();
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


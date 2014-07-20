using System.Collections.Concurrent;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Core.Settings.Models;
using Orchard.DisplayManagement.Shapes;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.Widgets.Filters;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.Tests.Modules.Widgets
{
    [TestFixture]
    public class WidgetFilterTests
    {
        #region Declarations and Setup

        private IContainer _container;
        private IResultFilter _filter;
        private IDictionary<WidgetPart, Func<Task<dynamic>>> _parts;
        private static ConcurrentBag<string> _renderedShapes;

        [SetUp]
        public void Init()
        {
            _parts = new Dictionary<WidgetPart, Func<Task<dynamic>>>(new WidgetCamparer());
            _renderedShapes = new ConcurrentBag<string>();

            var builder = new ContainerBuilder();

            builder.RegisterType<WidgetFilter>().As<IResultFilter>();

            var widgetsService = new Mock<IWidgetsService>();
            var workContextAccessor = new Mock<IWorkContextAccessor>();
            var workContext = new Mock<WorkContext>();
            var orchardService = new Mock<IOrchardServices>();
            var contentManager = new Mock<IContentManager>();
            var typedQuery = new Mock<IContentQuery<LayerPart>>();
            var contentQuery = new Mock<IContentQuery<ContentItem>>();
            var authorizer = new Mock<IAuthorizer>();
            var ruleManager = new Mock<IRuleManager>();

            widgetsService.Setup(s => s.GetWidgets(It.IsAny<int[]>())).Returns(() => _parts.Keys);
            workContext.Setup(c => c.GetState<dynamic>("Layout")).Returns(new CustomLayout());
            workContext.Setup(c => c.GetState<ISite>("CurrentSite")).Returns(CreateSiteSettings());
            workContext.Setup(c => c.GetState<string>("CurrentCulture")).Returns("en-us");
            workContextAccessor.Setup(w => w.GetContext(It.IsAny<HttpContextBase>())).Returns(workContext.Object);
            orchardService.Setup(o => o.ContentManager).Returns(contentManager.Object);
            contentManager.Setup(c => c.Query()).Returns(contentQuery.Object);
            contentQuery.Setup(cq => cq.ForPart<LayerPart>()).Returns(typedQuery.Object);
            typedQuery.Setup(q => q.ForType("Layer")).Returns(typedQuery.Object);
            typedQuery.Setup(q => q.List()).Returns(CreateLayerParts());
            contentManager.Setup(c => c.BuildDisplayAsync(It.IsAny<IContent>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IContent content, string d, string g) => _parts[(WidgetPart)content]());
            orchardService.Setup(o => o.Authorizer).Returns(authorizer.Object);
            authorizer.Setup(a => a.Authorize(Core.Contents.Permissions.ViewContent, It.IsAny<IContent>())).Returns(true);
            ruleManager.Setup(r => r.Matches(It.IsAny<string>())).Returns(true);

            builder.RegisterInstance(widgetsService.Object).As<IWidgetsService>();
            builder.RegisterInstance(workContextAccessor.Object).As<IWorkContextAccessor>();
            builder.RegisterInstance(orchardService.Object).As<IOrchardServices>();
            builder.RegisterInstance(ruleManager.Object).As<IRuleManager>();

            _container = builder.Build();
            _filter = _container.Resolve<IResultFilter>();
        }

        #endregion

        [Test]
        [TestCase(3)]
        [TestCase(10)]
        [TestCase(100)]
        public void WidgetFilter_WhenAsyncShapes_RendersAllWidgets(int count)
        {
            var context = CreateContext();
            var random = new Random(count);

            for (var i = 1; i <= count; i++) {
                var delay = random.Next(1, 5);
                AddWidgetPartAndShapeResult(i.ToString(), async () =>
                {
                    await Task.Delay(delay);
                    return new Shape();
                });
            }

            _filter.OnResultExecuting(context);

            Assert.AreEqual(count, _renderedShapes.Count, "Expected {0} shapes rendered", count);
            for (var i = 1; i <= count; i++) 
                Assert.Contains(i.ToString(), _renderedShapes, "Expected rendered shapes list to contain shape with position '{0}'", i);
        }

        [Test]
        [TestCase(3)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void WidgetFilter_WhenAsyncAndAsync_RendersAllWidgets(int count)
        {
            var context = CreateContext();
            var random = new Random(count);

            for (var i = 1; i <= count; i++)
            {
                if (i%2 == 0) {
                    var delay = random.Next(1, 5);
                    AddWidgetPartAndShapeResult(i.ToString(), async () => {
                        await Task.Delay(delay);
                        return new Shape();
                    });
                }
                else {
                    AddWidgetPartAndShapeResult(i.ToString(), () => Task.FromResult<dynamic>(new Shape()));
                }
                
            }

            _filter.OnResultExecuting(context);

            Assert.AreEqual(count, _renderedShapes.Count, "Expected {0} shapes rendered", count);
            for (var i = 1; i <= count; i++)
                Assert.Contains(i.ToString(), _renderedShapes, "Expected rendered shapes list to contain shape with position '{0}'. Async shape: {1}?", i, i % 2 == 0);
        }

        [Test]
        public void WidgetFilter_WhenShapeIsNull_RendersNotNullShapesWithNoException()
        {
            var context = CreateContext();
            AddWidgetPartAndShapeResult("1", async () =>
            {
                await Task.Delay(2);
                return new Shape();
            });
            AddWidgetPartAndShapeResult("2", async () =>
            {
                await Task.Delay(2);
                return null;
            });
            AddWidgetPartAndShapeResult("3", async () =>
            {
                await Task.Delay(2);
                return new Shape();
            });

            _filter.OnResultExecuting(context);

            Assert.AreEqual(2, _renderedShapes.Count, "Expected 3 shapes rendered");
            Assert.Contains("1", _renderedShapes, "Expected rendered shaped contain one with position '1'");
            Assert.Contains("3", _renderedShapes, "Expected rendered shaped contain one with position '3'");
        }

        [Test]
        public void WidgetFilter_WhenRenderThrows_ExpectExceptionNotDeadlock()
        {
            var context = CreateContext();
            AddWidgetPartAndShapeResult("1", async () =>
            {
                await Task.Delay(2);
                throw new Exception("From Shape Rendering");
            });
            AddWidgetPartAndShapeResult("2", async () =>
            {
                await Task.Delay(2);
                return new Shape();
            });
            AddWidgetPartAndShapeResult("3", async () =>
            {
                await Task.Delay(2);
                return new Shape();
            });

            Assert.Throws<AggregateException>(() => _filter.OnResultExecuting(context));

            Assert.AreEqual(2, _renderedShapes.Count, "Expected 3 shapes rendered");
            Assert.Contains("2", _renderedShapes, "Expected rendered shaped contain one with position '2'");
            Assert.Contains("3", _renderedShapes, "Expected rendered shaped contain one with position '3'");
        }

        [Test]
        [TestCase(3)]
        [TestCase(10)]
        [TestCase(100)]
        public void WidgetFilter_WithSyncShape_RendersAllWidgets(int count)
        {
            var context = CreateContext();

            for (var i = 1; i <= count; i++)
            {
                AddWidgetPartAndShapeResult(i.ToString(), () => Task.FromResult<dynamic>(new Shape()));
            }

            _filter.OnResultExecuting(context);

            Assert.AreEqual(count, _renderedShapes.Count, "Expected {0} shapes rendered", count);
            for (var i = 1; i <= count; i++)
                Assert.Contains(i.ToString(), _renderedShapes, "Expected rendered shapes list to contain shape with position '{}'", i);
        }


        #region Private Methods

        private void AddWidgetPartAndShapeResult(string position, Func<Task<dynamic>> widgetPartResult)
        {
            var contentItem = new ContentItem();
            var commonPart = new StubCommonPart();
            var infoSetPart = new InfosetPart();
            var part = new WidgetPart { Record = new WidgetPartRecord { Position = position } };
            contentItem.Weld(commonPart);
            contentItem.Weld(part);
            contentItem.Weld(infoSetPart);
            _parts.Add(part, widgetPartResult);
        }

        private IEnumerable<LayerPart> CreateLayerParts()
        {
            var contentItem = new ContentItem();
            var commonPart = new StubCommonPart();
            var infoSetPart = new InfosetPart();
            var part = new LayerPart
            {
                Record = new LayerPartRecord
                {
                    LayerRule = "true",
                    Name = "Layer"
                }
            };

            contentItem.Weld(commonPart);
            contentItem.Weld(part);
            contentItem.Weld(infoSetPart);
            return new[] { part };
        }

        private SiteSettingsPart CreateSiteSettings()
        {
            var contentItem = new ContentItem();
            var infoSetPart = new InfosetPart();
            var part = new SiteSettingsPart();

            contentItem.Weld(part);
            contentItem.Weld(infoSetPart);
            return part;
        }

        private static ResultExecutingContext CreateContext()
        {
            var httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(c => c.Items).Returns(new HybridDictionary {
                {typeof (ThemeFilter), null}
            });


            return new ResultExecutingContext
            {
                Result = new ViewResult(),
                RequestContext = new RequestContext
                {
                    HttpContext = httpContext.Object
                }
            };
        }

        #endregion

        #region Private Classes

        public class CustomShape
        {

            public void Add(dynamic shape, string position)
            {
                _renderedShapes.Add(position);
            }
        }

        public class CustomZones
        {
            public dynamic this[string zone]
            {
                get { return new CustomShape(); }
            }
        }

        public class CustomLayout
        {
            public dynamic Zones = new CustomZones();
        }

        private class WidgetCamparer : IEqualityComparer<WidgetPart>
        {
            public bool Equals(WidgetPart x, WidgetPart y)
            {
                return x.Position.Equals(y.Position);
            }

            public int GetHashCode(WidgetPart obj)
            {
                return obj.Position.GetHashCode();
            }
        }

        private class StubCommonPart : ContentPart, ICommonPart {
            public IUser Owner {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IContent Container {
                get { return new ContentItem(); }
                set { throw new NotImplementedException(); }
            }

            public DateTime? CreatedUtc {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public DateTime? PublishedUtc {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public DateTime? ModifiedUtc {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public DateTime? VersionCreatedUtc {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public DateTime? VersionPublishedUtc {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public DateTime? VersionModifiedUtc {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }

        #endregion
    }
}
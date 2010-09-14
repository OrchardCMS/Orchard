using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Drivers.Coordinators;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.AutofacUtil;

namespace Orchard.Tests.ContentManagement.Handlers.Coordinators {
    [TestFixture]
    public class ContentPartDriverCoordinatorTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            //builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.RegisterType<ContentPartDriverCoordinator>().As<IContentHandler>();
            builder.RegisterType<ShapeHelperFactory>().As<IShapeHelperFactory>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterInstance(new Mock<IContentDefinitionManager>().Object);
            _container = builder.Build();
        }

        [Test]
        public void DriverHandlerShouldNotThrowException() {
            var contentHandler = _container.Resolve<IContentHandler>();
            contentHandler.BuildDisplayShape(null);
        }

        [Test]
        public void AllDriversShouldBeCalled() {
            var driver1 = new Mock<IContentPartDriver>();
            var driver2 = new Mock<IContentPartDriver>();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(driver1.Object);
            builder.RegisterInstance(driver2.Object);
            builder.Update(_container);
            var contentHandler = _container.Resolve<IContentHandler>();

            var contentItem = new ContentItem();
            var context = new BuildDisplayModelContext(contentItem, "", null, null);

            driver1.Verify(x => x.BuildDisplayShape(context), Times.Never());
            driver2.Verify(x => x.BuildDisplayShape(context), Times.Never());
            contentHandler.BuildDisplayShape(context);
            driver1.Verify(x => x.BuildDisplayShape(context));
            driver2.Verify(x => x.BuildDisplayShape(context));
        }

        [Test, Ignore("no implementation for IZoneCollection")]
        public void TestDriverCanAddDisplay() {
            var driver = new StubPartDriver();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(driver).As<IContentPartDriver>();
            builder.Update(_container);
            var contentHandler = _container.Resolve<IContentHandler>();
            var shapeHelperFactory = _container.Resolve<IShapeHelperFactory>();

            var contentItem = new ContentItem();
            contentItem.Weld(new StubPart { Foo = new[] { "a", "b", "c" } });

            var ctx = new BuildDisplayModelContext(contentItem, "", null, null);
            var context = shapeHelperFactory.CreateHelper().Context(ctx);
            Assert.That(context.TopMeta, Is.Null);
            contentHandler.BuildDisplayShape(ctx);
            Assert.That(context.TopMeta, Is.Not.Null);
            Assert.That(context.TopMeta.Count == 1);
        }

        public class StubPartDriver : ContentPartDriver<StubPart> {
            protected override string Prefix {
                get { return "Stub"; }
            }

            protected override DriverResult Display(StubPart part, string displayType) {
                var viewModel = new StubViewModel { Foo = string.Join(",", part.Foo) };
                if (displayType.StartsWith("Summary"))
                    return ContentPartTemplate(viewModel, "StubViewModelTerse").Location("TopMeta");

                return ContentPartTemplate(viewModel).Location("TopMeta");
            }

            protected override DriverResult Editor(StubPart part) {
                var viewModel = new StubViewModel { Foo = string.Join(",", part.Foo) };
                return ContentPartTemplate(viewModel).Location("last", "10");
            }

            protected override DriverResult Editor(StubPart part, IUpdateModel updater) {
                var viewModel = new StubViewModel { Foo = string.Join(",", part.Foo) };
                updater.TryUpdateModel(viewModel, Prefix, null, null);
                part.Foo = viewModel.Foo.Split(new[] { ',' }).Select(x => x.Trim()).ToArray();
                return ContentPartTemplate(viewModel).Location("last", "10");
            }
        }

        public class StubPart : ContentPart {
            public string[] Foo { get; set; }
        }

        public class StubViewModel {
            [Required]
            public string Foo { get; set; }
        }
    }
}

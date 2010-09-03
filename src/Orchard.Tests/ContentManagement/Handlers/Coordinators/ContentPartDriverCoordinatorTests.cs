using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Drivers.Coordinators;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.AutofacUtil;
using Orchard.Mvc.ViewModels;
using Orchard.Tests.Utility;
using Orchard.UI.Zones;

namespace Orchard.Tests.ContentManagement.Handlers.Coordinators {
    [TestFixture]
    public class ContentPartDriverCoordinatorTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            //builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.RegisterType<ContentPartDriverCoordinator>().As<IContentHandler>();
            builder.RegisterAutoMocking();
            _container = builder.Build();
        }

        [Test]
        public void DriverHandlerShouldNotThrowException() {
            var contentHandler = _container.Resolve<IContentHandler>();
            contentHandler.BuildDisplayModel(null);
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

            var ctx = new BuildDisplayModelContext(new ContentItemViewModel(new ContentItem()), null);

            driver1.Verify(x => x.BuildDisplayModel(ctx), Times.Never());
            contentHandler.BuildDisplayModel(ctx);
            driver1.Verify(x => x.BuildDisplayModel(ctx));
        }

        [Test]
        public void TestDriverCanAddDisplay() {
            var driver = new StubPartDriver();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(driver).As<IContentPartDriver>();
            builder.Update(_container);

            var contentHandler = _container.Resolve<IContentHandler>();

            var item = new ContentItem();
            item.Weld(new StubPart { Foo = new[] { "a", "b", "c" } });

            var ctx = new BuildDisplayModelContext(new ContentItemViewModel(item), "");
            Assert.That(ctx.ViewModel.Zones.Count(), Is.EqualTo(0));
            contentHandler.BuildDisplayModel(ctx);
            Assert.That(ctx.ViewModel.Zones.Count(), Is.EqualTo(1));
            Assert.That(ctx.ViewModel.Zones.Single().Key, Is.EqualTo("topmeta"));
            Assert.That(ctx.ViewModel.Zones.Single().Value.Items.OfType<ContentPartDisplayZoneItem>().Single().Prefix, Is.EqualTo("Stub"));

        }

        public class StubPartDriver : ContentPartDriver<StubPart> {
            protected override string Prefix {
                get { return "Stub"; }
            }

            protected override DriverResult Display(StubPart part, string displayType) {
                var viewModel = new StubViewModel { Foo = string.Join(",", part.Foo) };
                if (displayType.StartsWith("Summary"))
                    return ContentPartTemplate(viewModel, "StubViewModelTerse").Location("topmeta");

                return ContentPartTemplate(viewModel).Location("topmeta");
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

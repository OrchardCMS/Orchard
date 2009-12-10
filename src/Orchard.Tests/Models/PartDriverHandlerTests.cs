using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Builder;
using Autofac.Modules;
using Moq;
using NUnit.Framework;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.Tests.Models {
    [TestFixture]
    public class PartDriverHandlerTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.Register<PartDriverHandler>().As<IContentHandler>();
            _container = builder.Build();
        }

        [Test]
        public void DriverHandlerShouldNotThrowException() {
            var contentHandler = _container.Resolve<IContentHandler>();
            contentHandler.BuildDisplayModel(null);
        }

        [Test]
        public void AllDriversShouldBeCalled() {
            var driver1 = new Mock<IPartDriver>();
            var driver2 = new Mock<IPartDriver>();
            _container.Build(x => {
                x.Register(driver1.Object);
                x.Register(driver2.Object);
            });
            var contentHandler = _container.Resolve<IContentHandler>();

            var ctx = new BuildDisplayModelContext(new ItemDisplayModel(new ContentItem()), null, null);

            driver1.Verify(x => x.BuildDisplayModel(ctx), Times.Never());
            contentHandler.BuildDisplayModel(ctx);
            driver1.Verify(x => x.BuildDisplayModel(ctx));
        }

        [Test]
        public void TestDriverCanAddDisplay() {
            var driver = new StubDriver();
            _container.Build(x => x.Register(driver).As<IPartDriver>());

            var contentHandler = _container.Resolve<IContentHandler>();

            var item = new ContentItem();
            item.Weld(new StubPart());

            var ctx = new BuildDisplayModelContext(new ItemDisplayModel(item), null, null);
            Assert.That(ctx.DisplayModel.Displays.Count(), Is.EqualTo(0));
            contentHandler.BuildDisplayModel(ctx);
            Assert.That(ctx.DisplayModel.Displays.Count(), Is.EqualTo(1));
            Assert.That(ctx.DisplayModel.Displays.Single().Prefix, Is.EqualTo("Stub"));

        }

        private class StubDriver : PartDriver<StubPart> {
            protected override string Prefix {
                get { return "Stub"; }
            }

            protected override DriverResult Display(StubPart part, string groupName, string displayType) {
                var viewModel = new StubViewModel { Foo = string.Join(",", part.Foo) };
                return PartialView(viewModel);
            }

            protected override DriverResult Editor(StubPart part, string groupName) {
                var viewModel = new StubViewModel { Foo = string.Join(",", part.Foo) };
                return PartialView(viewModel);
            }

            protected override DriverResult Editor(StubPart part, string groupName, IUpdateModel updater) {
                var viewModel = new StubViewModel { Foo = string.Join(",", part.Foo) };
                updater.TryUpdateModel(viewModel, Prefix, null, null);
                part.Foo = viewModel.Foo.Split(new[] {','}).Select(x => x.Trim()).ToArray();
                return PartialView(viewModel);
            }
        }

        private class StubPart : ContentPart {
            public string[] Foo { get; set; }
        }
        private class StubViewModel {
            [Required]
            public string Foo { get; set; }
        }
    }

}

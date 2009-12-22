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
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;

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

            var ctx = new BuildDisplayModelContext(new ItemDisplayModel(new ContentItem()), null);

            driver1.Verify(x => x.BuildDisplayModel(ctx), Times.Never());
            contentHandler.BuildDisplayModel(ctx);
            driver1.Verify(x => x.BuildDisplayModel(ctx));
        }

        [Test]
        public void TestDriverCanAddDisplay() {
            var driver = new StubPartDriver();
            _container.Build(x => x.Register(driver).As<IPartDriver>());

            var contentHandler = _container.Resolve<IContentHandler>();

            var item = new ContentItem();
            item.Weld(new StubPart { Foo = new[] { "a", "b", "c" } });

            var ctx = new BuildDisplayModelContext(new ItemDisplayModel(item), "");
            Assert.That(ctx.DisplayModel.Displays.Count(), Is.EqualTo(0));
            contentHandler.BuildDisplayModel(ctx);
            Assert.That(ctx.DisplayModel.Displays.Count(), Is.EqualTo(1));
            Assert.That(ctx.DisplayModel.Displays.Single().Prefix, Is.EqualTo("Stub"));

        }

        public class StubPartDriver : PartDriver<StubPart> {
            protected override string Prefix {
                get { return "Stub"; }
            }

            protected override DriverResult Display(StubPart part, string displayType) {
                var viewModel = new StubViewModel { Foo = string.Join(",", part.Foo) };
                if (displayType.StartsWith("Summary"))
                    return PartTemplate(viewModel, "StubViewModelTerse").Location("topmeta");

                return PartTemplate(viewModel).Location("topmeta");
            }

            protected override DriverResult Editor(StubPart part) {
                var viewModel = new StubViewModel { Foo = string.Join(",", part.Foo) };
                return PartTemplate(viewModel).Location("last", "10");
            }

            protected override DriverResult Editor(StubPart part, IUpdateModel updater) {
                var viewModel = new StubViewModel { Foo = string.Join(",", part.Foo) };
                updater.TryUpdateModel(viewModel, Prefix, null, null);
                part.Foo = viewModel.Foo.Split(new[] { ',' }).Select(x => x.Trim()).ToArray();
                return PartTemplate(viewModel).Location("last", "10");
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

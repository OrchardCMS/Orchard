using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.Tests.Stubs;
using System.Web;

namespace Orchard.Tests.Localization {
    [TestFixture]
    public class TextTests {
        private IContainer _container;
        private IText _text;

        [SetUp]
        public void Init() {
            var mockLocalizedManager = new Mock<ILocalizedStringManager>();
            mockLocalizedManager
                .Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("foo {0}");

            var builder = new ContainerBuilder();
            builder.RegisterInstance(new StubCultureSelector("fr-CA")).As<ICultureSelector>();
            builder.RegisterInstance(new StubWorkContext()).As<WorkContext>();
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterInstance(mockLocalizedManager.Object);
            builder.RegisterType<Orchard.Localization.Text>().As<IText>().WithParameter(new NamedParameter("scope", "scope"));
            _container = builder.Build();
            _text = _container.Resolve<IText>();
        }

        [Test]
        public void TextHtmlEncodeAllArguments() {
            Assert.That(_text.Get("foo {0}", "bar").Text, Is.EqualTo("foo bar"));
            Assert.That(_text.Get("foo {0}", "<bar>").Text, Is.EqualTo("foo &lt;bar&gt;"));
        }

        [Test]
        public void TextDoesEncodeHtmlEncodedArguments()
        {
            Assert.That(_text.Get("foo {0}", new HtmlString("bar")).Text, Is.EqualTo("foo bar"));
            Assert.That(_text.Get("foo {0}", new HtmlString("<bar>")).Text, Is.EqualTo("foo <bar>"));
        }
    }
}
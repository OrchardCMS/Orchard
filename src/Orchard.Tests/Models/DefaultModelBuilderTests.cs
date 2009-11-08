using System;
using Autofac;
using Autofac.Builder;
using Autofac.Modules;
using NUnit.Framework;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Tests.Models.Stubs;

namespace Orchard.Tests.Models {
    [TestFixture]
    public class DefaultModelBuilderTests {
        private IContainer _container;
        private IModelManager _manager;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ImplicitCollectionSupportModule());
            builder.Register<DefaultModelManager>().As<IModelManager>();
            builder.Register<AlphaDriver>().As<IModelDriver>();
            builder.Register<BetaDriver>().As<IModelDriver>();
            builder.Register<FlavoredDriver>().As<IModelDriver>();
            builder.Register<StyledDriver>().As<IModelDriver>();

            _container = builder.Build();
            _manager = _container.Resolve<IModelManager>();
        }

        [Test]
        public void AlphaDriverShouldWeldItsPart() {
            var foo = _manager.New("alpha");
            
            Assert.That(foo.Is<Alpha>(), Is.True);
            Assert.That(foo.As<Alpha>(), Is.Not.Null);
            Assert.That(foo.Is<Beta>(), Is.False);
            Assert.That(foo.As<Beta>(), Is.Null);
        }

        [Test]
        public void StronglyTypedNewShouldTypeCast() {
            var foo = _manager.New<Alpha>("alpha");
            Assert.That(foo, Is.Not.Null);
            Assert.That(foo.GetType(), Is.EqualTo(typeof(Alpha)));
        }

        [Test, ExpectedException(typeof(InvalidCastException))]
        public void StronglyTypedNewShouldThrowCastExceptionIfNull() {
            var foo = _manager.New<Beta>("alpha");
        }

        [Test]
        public void AlphaIsFlavoredAndStyledAndBetaIsFlavoredOnly() {
            var alpha = _manager.New<Alpha>("alpha");
            var beta = _manager.New<Beta>("beta");

            Assert.That(alpha.Is<Flavored>(), Is.True);
            Assert.That(alpha.Is<Styled>(), Is.True);
            Assert.That(beta.Is<Flavored>(), Is.True);
            Assert.That(beta.Is<Styled>(), Is.False);
        }

    }
}

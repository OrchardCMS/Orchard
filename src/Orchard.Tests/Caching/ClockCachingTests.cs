using System;
using Autofac;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Caching {
    [TestFixture]
    public class ClockCachingTests {
        private IContainer _container;
        private ICacheManager _cacheManager;
        private StubClock _clock;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new CacheModule());
            builder.RegisterType<DefaultCacheManager>().As<ICacheManager>();
            builder.RegisterType<DefaultCacheHolder>().As<ICacheHolder>().SingleInstance();
            builder.RegisterType<DefaultCacheContextAccessor>().As<ICacheContextAccessor>();
            builder.RegisterInstance<IClock>(_clock = new StubClock());
            _container = builder.Build();
            _cacheManager = _container.Resolve<ICacheManager>(new TypedParameter(typeof(Type), GetType()));
        }

        [Test]
        public void WhenAbsoluteShouldHandleAbsoluteTime() {
            var inOneSecond = _clock.UtcNow.AddSeconds(1);
            var cached = 0;

            // each call after the specified datetime will be reevaluated
            Func<int> retrieve = ()
                => _cacheManager.Get("testItem",
                        ctx => {
                            ctx.Monitor(_clock.WhenUtc(inOneSecond));
                            return ++cached;
                        });

            Assert.That(retrieve(), Is.EqualTo(1));

            for ( var i = 0; i < 10; i++ ) {
                Assert.That(retrieve(), Is.EqualTo(1));
            }

            _clock.Advance(TimeSpan.FromSeconds(1));

            Assert.That(retrieve(), Is.EqualTo(2));
            Assert.That(retrieve(), Is.EqualTo(3));
            Assert.That(retrieve(), Is.EqualTo(4));
        }

        [Test]
        public void WhenAbsoluteShouldHandleAbsoluteTimeSpan() {
            var cached = 0;

            // each cached value has a lifetime of the specified duration
            Func<int> retrieve = ()
                => _cacheManager.Get("testItem",
                        ctx => {
                            ctx.Monitor(_clock.When(TimeSpan.FromSeconds(1)));
                            return ++cached;
                        });

            Assert.That(retrieve(), Is.EqualTo(1));

            for ( var i = 0; i < 10; i++ ) {
                Assert.That(retrieve(), Is.EqualTo(1));
            }

            _clock.Advance(TimeSpan.FromSeconds(1));

            for ( var i = 0; i < 10; i++ ) {
                Assert.That(retrieve(), Is.EqualTo(2));
            }

            _clock.Advance(TimeSpan.FromSeconds(1));

            for ( var i = 0; i < 10; i++ ) {
                Assert.That(retrieve(), Is.EqualTo(3));
            }
        }
    }
}
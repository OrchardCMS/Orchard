using Autofac;
using NUnit.Framework;
using Orchard.Caching;

namespace Orchard.Tests.Caching {
    [TestFixture]
    public class CacheTests {
        private IContainer _container;
        private ICacheManager _cacheManager;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultCacheManager>().As<ICacheManager>();
            _container = builder.Build();
            _cacheManager = _container.Resolve<ICacheManager>();
        }

        [Test]
        public void CacheManagerShouldReturnCacheItem() {
            var result = _cacheManager.Get("testItem", ctx => "testResult");
            Assert.That(result, Is.EqualTo("testResult"));
        }

        [Test]
        public void CacheManagerShouldReturnExistingCacheItem() {
            _cacheManager.Get("testItem", ctx => "testResult");
            var result = _cacheManager.Get("testItem", ctx => "");
            Assert.That(result, Is.EqualTo("testResult"));
        }
    }
}

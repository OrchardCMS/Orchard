using NUnit.Framework;
using Orchard.Caching;
using Orchard.Environment;
using Autofac;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.WebSite;
using Orchard.Services;

namespace Orchard.Tests.Caching {
    [TestFixture]
    public class CacheScopeTests {
        private IContainer _hostContainer;

        [SetUp]
        public void Init() {
            _hostContainer = OrchardStarter.CreateHostContainer(builder => {
                builder.RegisterType<Alpha>().InstancePerDependency();
            });

        }

        public class Alpha {
            public ICacheManager CacheManager { get; set; }

            public Alpha(ICacheManager cacheManager) {
                CacheManager = cacheManager;
            }
        }

        [Test]
        public void ComponentsAtHostLevelHaveAccessToCache() {
            var alpha = _hostContainer.Resolve<Alpha>();
            Assert.That(alpha.CacheManager, Is.Not.Null);
        }

        [Test]
        public void HostLevelHasAccessToGlobalVolatileProviders() {
            Assert.That(_hostContainer.Resolve<IWebSiteFolder>(), Is.Not.Null);
            Assert.That(_hostContainer.Resolve<IAppDataFolder>(), Is.Not.Null);
            Assert.That(_hostContainer.Resolve<IClock>(), Is.Not.Null);
        }

    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            builder.RegisterModule(new CacheModule());
            builder.RegisterType<DefaultCacheManager>().As<ICacheManager>();
            builder.RegisterType<DefaultCacheHolder>().As<ICacheHolder>().SingleInstance();
            builder.RegisterType<DefaultCacheContextAccessor>().As<ICacheContextAccessor>();
            _container = builder.Build();
            _cacheManager = _container.Resolve<ICacheManager>(new TypedParameter(typeof(Type), GetType()));
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

        [Test]
        public void CacheModuleProvidesTypeSpecificManager() {
            var scope = _container.BeginLifetimeScope(builder => {
                builder.RegisterModule(new CacheModule());
                builder.RegisterType<ComponentOne>();
                builder.RegisterType<ComponentTwo>();
            });

            var c1 = scope.Resolve<ComponentOne>();
            var c2 = scope.Resolve<ComponentTwo>();
            var w1a = c1.CacheManager.Get("hello", ctx => "world1");
            var w1b = c1.CacheManager.Get("hello", ctx => "world2");
            var w2a = c2.CacheManager.Get("hello", ctx => "world3");
            var w2b = c2.CacheManager.Get("hello", ctx => "world4");

            Assert.That(w1a, Is.EqualTo("world1"));
            Assert.That(w1b, Is.EqualTo("world1"));
            Assert.That(w2a, Is.EqualTo("world3"));
            Assert.That(w2b, Is.EqualTo("world3"));

            var c3 = scope.Resolve<ComponentOne>();
            var c4 = scope.Resolve<ComponentTwo>();
            var w3a = c3.CacheManager.Get("hello", ctx => "world5");
            var w3b = c3.CacheManager.Get("hello", ctx => "world6");
            var w4a = c4.CacheManager.Get("hello", ctx => "world7");
            var w4b = c4.CacheManager.Get("hello", ctx => "world8");

            Assert.That(w3a, Is.EqualTo("world1"));
            Assert.That(w3b, Is.EqualTo("world1"));
            Assert.That(w4a, Is.EqualTo("world3"));
            Assert.That(w4b, Is.EqualTo("world3"));

            Assert.That(c1.CacheManager,
                Is.Not.SameAs(c3.CacheManager));

            Assert.That(c1.CacheManager.GetCache<string, string>(),
                Is.SameAs(c3.CacheManager.GetCache<string, string>()));

            Assert.That(c1.CacheManager,
                Is.Not.SameAs(c2.CacheManager));

            Assert.That(c1.CacheManager.GetCache<string, string>(),
                Is.Not.SameAs(c2.CacheManager.GetCache<string, string>()));
        }

        [Test]
        public void CacheManagerIsNotBlocking() {
            var hits = 0;
            string result = "";
            string key = "key";

            var e1 = new ManualResetEvent(false);
            var e2 = new ManualResetEvent(false);
            var e3 = new ManualResetEvent(false);

            // task1 is started first, when inside the lambda, we are waiting 
            // for the test to give the green light. Then we unblock the task2
            var task1 = Task.Run(() => {
                result = _cacheManager.Get(key, ctx => {
                    e1.WaitOne(TimeSpan.FromSeconds(5));
                    hits++;
                    e2.Set();
                    e3.WaitOne(TimeSpan.FromSeconds(5));

                    return "testResult";
                });
            });

            // task2 is called once task1 is inside the lambda, ensuring it's not blocking.
            var task2 = Task.Run(() => {
                e2.WaitOne(TimeSpan.FromSeconds(5));
                result = _cacheManager.Get(key, ctx => {
                    hits++;
                    e3.Set();
                    return "testResult";
                });
            });

            e1.Set();
            Task.WaitAll(task1, task2);

            Assert.That(result, Is.EqualTo("testResult"));
            Assert.That(hits, Is.EqualTo(2));
        }

        [Test]
        public void CacheManagerIsBlocking() {
            var hits = 0;
            string result = "";
            string key = "key";

            var e1 = new ManualResetEvent(false);
            var e2 = new ManualResetEvent(false);

            // task1 is started first, when inside the lambda, we are waiting 
            // for the test to give the green light. Then we unblock the task2
            var task1 = Task.Run(() => {
                result = _cacheManager.Get(key, true, ctx => {
                    e1.WaitOne(TimeSpan.FromSeconds(5));
                    hits++;
                    e2.Set();
                    return "testResult";
                });
            });

            // task2 is called once task1 is inside the lambda. Here we expect the lamda not to be called.
            var task2 = Task.Run(() => {
                e2.WaitOne(TimeSpan.FromSeconds(5));
                result = _cacheManager.Get(key, true, ctx => {
                    hits++;
                    return "testResult";
                });
            });

            e1.Set();
            Task.WaitAll(task1, task2);

            Assert.That(result, Is.EqualTo("testResult"));
            Assert.That(hits, Is.EqualTo(1));
        }

        class ComponentOne {
            public ICacheManager CacheManager { get; set; }

            public ComponentOne(ICacheManager cacheManager) {
                CacheManager = cacheManager;
            }
        }

        class ComponentTwo {
            public ICacheManager CacheManager { get; set; }

            public ComponentTwo(ICacheManager cacheManager) {
                CacheManager = cacheManager;
            }
        }
    }
}
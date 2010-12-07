using System.Web;
using Autofac;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Mvc;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;

namespace Orchard.Tests.Environment {
    [TestFixture]
    public class DefaultWorkContextAccessorTests : ContainerTestBase {

        HttpContextBase _httpContextCurrent;

        public override void Init() {
            _httpContextCurrent = null;
            base.Init();
        }

        protected override void Register(ContainerBuilder builder) {
            builder.RegisterModule(new WorkContextModule());
            builder.RegisterType<WorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterAutoMocking();
        }

        protected override void Resolve(ILifetimeScope container) {
            container.Mock<IHttpContextAccessor>()
                .Setup(x => x.Current())
                .Returns(() => _httpContextCurrent);
        }

        [Test]
        public void ScopeIsCreatedAndCanBeRetrievedFromHttpContextBase() {
            var accessor = _container.Resolve<IWorkContextAccessor>();
            var httpContext = new StubHttpContext();
            
            var workContextScope = accessor.CreateWorkContextScope(httpContext);
            Assert.That(workContextScope.WorkContext, Is.Not.Null);

            var workContext = accessor.GetContext(httpContext);
            Assert.That(workContext, Is.SameAs(workContextScope.WorkContext));
        }

        [Test]
        public void DifferentHttpContextWillHoldDifferentWorkContext() {
            var accessor = _container.Resolve<IWorkContextAccessor>();
            var httpContext1 = new StubHttpContext();
            var workContextScope1 = accessor.CreateWorkContextScope(httpContext1);
            var workContext1 = accessor.GetContext(httpContext1);

            var httpContext2 = new StubHttpContext();
            var workContextScope2 = accessor.CreateWorkContextScope(httpContext2);
            var workContext2 = accessor.GetContext(httpContext2);

            Assert.That(workContext1, Is.Not.Null);
            Assert.That(workContext1, Is.SameAs(workContextScope1.WorkContext));
            Assert.That(workContext2, Is.Not.Null);
            Assert.That(workContext2, Is.SameAs(workContextScope2.WorkContext));
            Assert.That(workContext1, Is.Not.SameAs(workContext2));
        }

        [Test]
        public void ContextIsNullAfterDisposingScope() {
            var accessor = _container.Resolve<IWorkContextAccessor>();
            var httpContext = new StubHttpContext();

            Assert.That(accessor.GetContext(httpContext), Is.Null);

            var scope = accessor.CreateWorkContextScope(httpContext);
            Assert.That(accessor.GetContext(httpContext), Is.Not.Null);
            
            scope.Dispose();
            Assert.That(accessor.GetContext(httpContext), Is.Null);
        }

        [Test]
        public void DifferentChildScopesWillNotCollideInTheSameHttpContext() {
            var shell1 = _container.BeginLifetimeScope();
            var accessor1 = shell1.Resolve<IWorkContextAccessor>();

            var shell2 = _container.BeginLifetimeScope();
            var accessor2 = shell2.Resolve<IWorkContextAccessor>();

            var httpContext = new StubHttpContext();

            Assert.That(accessor1.GetContext(httpContext), Is.Null);
            Assert.That(accessor2.GetContext(httpContext), Is.Null);

            var scope1 = accessor1.CreateWorkContextScope(httpContext);
            Assert.That(accessor1.GetContext(httpContext), Is.Not.Null);
            Assert.That(accessor2.GetContext(httpContext), Is.Null);

            var scope2 = accessor2.CreateWorkContextScope(httpContext);
            Assert.That(accessor1.GetContext(httpContext), Is.Not.Null);
            Assert.That(accessor2.GetContext(httpContext), Is.Not.Null);

            scope1.Dispose();
            Assert.That(accessor1.GetContext(httpContext), Is.Null);
            Assert.That(accessor2.GetContext(httpContext), Is.Not.Null);

            scope2.Dispose();
            Assert.That(accessor1.GetContext(httpContext), Is.Null);
            Assert.That(accessor2.GetContext(httpContext), Is.Null);
        }


        [Test]
        public void FunctionsByDefaultAgainstAmbientHttpContext() {
            var accessor = _container.Resolve<IWorkContextAccessor>();

            var explicitHttpContext = new StubHttpContext();
            var ambientHttpContext = new StubHttpContext();

            _httpContextCurrent = ambientHttpContext;

            Assert.That(accessor.GetContext(), Is.Null);
            Assert.That(accessor.GetContext(ambientHttpContext), Is.Null);
            Assert.That(accessor.GetContext(explicitHttpContext), Is.Null);

            var scope = accessor.CreateWorkContextScope();
            Assert.That(accessor.GetContext(), Is.Not.Null);
            Assert.That(accessor.GetContext(ambientHttpContext), Is.Not.Null);
            Assert.That(accessor.GetContext(explicitHttpContext), Is.Null);
            Assert.That(accessor.GetContext(), Is.SameAs(accessor.GetContext(ambientHttpContext)));

            _httpContextCurrent = explicitHttpContext;
            Assert.That(accessor.GetContext(), Is.Null);

            _httpContextCurrent = ambientHttpContext;
            Assert.That(accessor.GetContext(), Is.Not.Null);

            scope.Dispose();
            Assert.That(accessor.GetContext(), Is.Null);
        }


        [Test]
        public void StillFunctionsWithoutAmbientHttpContext() {
            var accessor = _container.Resolve<IWorkContextAccessor>();

            Assert.That(accessor.GetContext(), Is.Null);

            var scope = accessor.CreateWorkContextScope();
            Assert.That(accessor.GetContext(), Is.Not.Null);

            scope.Dispose();
            Assert.That(accessor.GetContext(), Is.Null);
        }

        [Test]
        public void DifferentChildScopesWillNotCollideWithoutAmbientHttpContext() {
            var shell1 = _container.BeginLifetimeScope();
            var accessor1 = shell1.Resolve<IWorkContextAccessor>();

            var shell2 = _container.BeginLifetimeScope();
            var accessor2 = shell2.Resolve<IWorkContextAccessor>();

            Assert.That(accessor1.GetContext(), Is.Null);
            Assert.That(accessor2.GetContext(), Is.Null);

            var scope1 = accessor1.CreateWorkContextScope();
            Assert.That(accessor1.GetContext(), Is.Not.Null);
            Assert.That(accessor2.GetContext(), Is.Null);

            var scope2 = accessor2.CreateWorkContextScope();
            Assert.That(accessor1.GetContext(), Is.Not.Null);
            Assert.That(accessor2.GetContext(), Is.Not.Null);

            scope1.Dispose();
            Assert.That(accessor1.GetContext(), Is.Null);
            Assert.That(accessor2.GetContext(), Is.Not.Null);

            scope2.Dispose();
            Assert.That(accessor1.GetContext(), Is.Null);
            Assert.That(accessor2.GetContext(), Is.Null);
        }

    }
}

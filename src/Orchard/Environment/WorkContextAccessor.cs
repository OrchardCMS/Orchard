using System;
using System.Collections.Concurrent;
using System.Web;
using Autofac;
using Orchard.Mvc;

namespace Orchard.Environment {
    public class WorkContextAccessor : IWorkContextAccessor {
        readonly ILifetimeScope _lifetimeScope;

        readonly IHttpContextAccessor _httpContextAccessor;
        // a different symbolic key is used for each tenant.
        // this guarantees the correct accessor is being resolved.
        readonly object _workContextKey = new object();

        [ThreadStatic]
        static ConcurrentDictionary<object, WorkContext> _threadStaticContexts;

        public WorkContextAccessor(
            IHttpContextAccessor httpContextAccessor,
            ILifetimeScope lifetimeScope) {
            _httpContextAccessor = httpContextAccessor;
            _lifetimeScope = lifetimeScope;
        }

        public WorkContext GetContext(HttpContextBase httpContext) {
            return httpContext.Items[_workContextKey] as WorkContext;
        }

        public WorkContext GetContext() {
            var httpContext = _httpContextAccessor.Current();
            if (httpContext != null)
                return GetContext(httpContext);

            WorkContext workContext;
            return EnsureThreadStaticContexts().TryGetValue(_workContextKey, out workContext) ? workContext : null;
        }

        public IWorkContextScope CreateWorkContextScope(HttpContextBase httpContext) {
            var workLifetime = _lifetimeScope.BeginLifetimeScope("work");
            workLifetime.Resolve<WorkContextProperty<HttpContextBase>>().Value = httpContext;

            return new HttpContextScopeImplementation(
                workLifetime,
                httpContext,
                _workContextKey);
        }


        public IWorkContextScope CreateWorkContextScope() {
            var httpContext = _httpContextAccessor.Current();
            if (httpContext != null)
                return CreateWorkContextScope(httpContext);

            return new ThreadStaticScopeImplementation(
                _lifetimeScope.BeginLifetimeScope("work"),
                EnsureThreadStaticContexts(),
                _workContextKey);
        }

        static ConcurrentDictionary<object, WorkContext> EnsureThreadStaticContexts() {
            return _threadStaticContexts ?? (_threadStaticContexts = new ConcurrentDictionary<object, WorkContext>());
        }


        class HttpContextScopeImplementation : IWorkContextScope {
            readonly WorkContext _workContext;
            readonly Action _disposer;

            public HttpContextScopeImplementation(ILifetimeScope lifetimeScope, HttpContextBase httpContext, object workContextKey) {
                _workContext = lifetimeScope.Resolve<WorkContext>();
                httpContext.Items[workContextKey] = _workContext;
                _disposer = () => {
                    httpContext.Items.Remove(workContextKey);
                    lifetimeScope.Dispose();
                };
            }

            void IDisposable.Dispose() {
                _disposer();
            }

            public WorkContext WorkContext {
                get { return _workContext; }
            }

            public TService Resolve<TService>() {
                return WorkContext.Resolve<TService>();
            }

            public bool TryResolve<TService>(out TService service) {
                return WorkContext.TryResolve(out service);
            }
        }

        class ThreadStaticScopeImplementation : IWorkContextScope {
            readonly WorkContext _workContext;
            readonly Action _disposer;

            public ThreadStaticScopeImplementation(ILifetimeScope lifetimeScope, ConcurrentDictionary<object, WorkContext> contexts, object workContextKey) {
                _workContext = lifetimeScope.Resolve<WorkContext>();
                contexts.AddOrUpdate(workContextKey, _workContext, (a, b) => _workContext);
                _disposer = () => {
                    WorkContext removedContext;
                    contexts.TryRemove(workContextKey, out removedContext);
                    lifetimeScope.Dispose();
                };
            }

            void IDisposable.Dispose() {
                _disposer();
            }

            public WorkContext WorkContext {
                get { return _workContext; }
            }

            public TService Resolve<TService>() {
                return WorkContext.Resolve<TService>();
            }

            public bool TryResolve<TService>(out TService service) {
                return WorkContext.TryResolve(out service);
            }
        }
    }
}

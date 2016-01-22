using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web;
using Autofac;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;

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
            if (!httpContext.IsBackgroundContext())
                return GetContext(httpContext);

            WorkContext workContext;
            return EnsureThreadStaticContexts().TryGetValue(_workContextKey, out workContext) ? workContext : null;
        }

        public IWorkContextScope CreateWorkContextScope(HttpContextBase httpContext) {
            var workLifetime = _lifetimeScope.BeginLifetimeScope("work");
            workLifetime.Resolve<WorkContextProperty<HttpContextBase>>().Value = httpContext;

            var events = workLifetime.Resolve<IEnumerable<IWorkContextEvents>>();
            events.Invoke(e => e.Started(), NullLogger.Instance);

            return new HttpContextScopeImplementation(
                events,
                workLifetime,
                httpContext,
                _workContextKey);
        }


        public IWorkContextScope CreateWorkContextScope() {
            var httpContext = _httpContextAccessor.Current();
            if (!httpContext.IsBackgroundContext())
                return CreateWorkContextScope(httpContext);

            var workLifetime = _lifetimeScope.BeginLifetimeScope("work");

            var events = workLifetime.Resolve<IEnumerable<IWorkContextEvents>>();
            events.Invoke(e => e.Started(), NullLogger.Instance);

            return new ThreadStaticScopeImplementation(
                events,
                workLifetime,
                EnsureThreadStaticContexts(),
                _workContextKey);
        }

        static ConcurrentDictionary<object, WorkContext> EnsureThreadStaticContexts() {
            return _threadStaticContexts ?? (_threadStaticContexts = new ConcurrentDictionary<object, WorkContext>());
        }


        class HttpContextScopeImplementation : IWorkContextScope {
            readonly WorkContext _workContext;
            readonly Action _disposer;

            public HttpContextScopeImplementation(IEnumerable<IWorkContextEvents> events, ILifetimeScope lifetimeScope, HttpContextBase httpContext, object workContextKey) {
                _workContext = lifetimeScope.Resolve<WorkContext>();
                httpContext.Items[workContextKey] = _workContext;

                _disposer = () => {
                    events.Invoke(e => e.Finished(), NullLogger.Instance);

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

            public ThreadStaticScopeImplementation(IEnumerable<IWorkContextEvents> events, ILifetimeScope lifetimeScope, ConcurrentDictionary<object, WorkContext> contexts, object workContextKey) {
                _workContext = lifetimeScope.Resolve<WorkContext>();
                contexts.AddOrUpdate(workContextKey, _workContext, (a, b) => _workContext);

                _disposer = () => {
                    events.Invoke(e => e.Finished(), NullLogger.Instance);
  
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

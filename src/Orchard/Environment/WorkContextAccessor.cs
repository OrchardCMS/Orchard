using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
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
        private readonly string _workContextSlot;

        public WorkContextAccessor(
            IHttpContextAccessor httpContextAccessor,
            ILifetimeScope lifetimeScope) {
            _httpContextAccessor = httpContextAccessor;
            _lifetimeScope = lifetimeScope;
            _workContextSlot = "WorkContext." + Guid.NewGuid().ToString("n");
        }

        public WorkContext GetContext(HttpContextBase httpContext) {
            if (!httpContext.IsBackgroundContext())
                return httpContext.Items[_workContextKey] as WorkContext;

            var context = CallContext.LogicalGetData(_workContextSlot) as ObjectHandle;
            return context != null ? context.Unwrap() as WorkContext : null;
        }

        public WorkContext GetContext() {
            var httpContext = _httpContextAccessor.Current();
            if (!httpContext.IsBackgroundContext())
                return GetContext(httpContext);

            var context = CallContext.LogicalGetData(_workContextSlot) as ObjectHandle;
            return context != null ? context.Unwrap() as WorkContext : null;
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

            return new CallContextScopeImplementation(events, workLifetime, _workContextSlot);
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

        class CallContextScopeImplementation : IWorkContextScope {
            readonly WorkContext _workContext;
            readonly Action _disposer;

            public CallContextScopeImplementation(IEnumerable<IWorkContextEvents> events, ILifetimeScope lifetimeScope, string workContextSlot) {

                CallContext.LogicalSetData(workContextSlot, null);

                _workContext = lifetimeScope.Resolve<WorkContext>();
                var httpContext = lifetimeScope.Resolve<HttpContextBase>();
                _workContext.HttpContext = httpContext;

                CallContext.LogicalSetData(workContextSlot, new ObjectHandle(_workContext));

                _disposer = () => {
                    events.Invoke(e => e.Finished(), NullLogger.Instance);
                    CallContext.FreeNamedDataSlot(workContextSlot);
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

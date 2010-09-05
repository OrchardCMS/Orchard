using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using Orchard.Mvc;

namespace Orchard.Environment {
    public class DefaultWorkContextAccessor : IWorkContextAccessor {
        readonly ILifetimeScope _lifetimeScope;

        readonly IHttpContextAccessor _httpContextAccessor;
        // a different symbolic key is used for each tenant.
        // this guarantees the correct accessor is being resolved.
        readonly object _workContextKey = new object();

        [ThreadStatic]
        static ConcurrentDictionary<object, WorkContext> _threadStaticContexts;

        public DefaultWorkContextAccessor(
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

            var workLifetime = SpawnWorkLifetime(builder => {
                builder.Register(ctx => httpContext)
                    .As<HttpContextBase>();

                builder.Register(ctx => new WorkContextImplementation(ctx))
                    .As<WorkContext>()
                    .InstancePerMatchingLifetimeScope("work");
            });
            return new HttpContextScopeImplementation(
                workLifetime,
                httpContext,
                _workContextKey);
        }


        public IWorkContextScope CreateWorkContextScope() {
            var httpContext = _httpContextAccessor.Current();
            if (httpContext != null)
                return CreateWorkContextScope(httpContext);

            var workLifetime = SpawnWorkLifetime(builder => {
                builder.Register(ctx => httpContext)
                    .As<HttpContextBase>();

                builder.Register(ctx => new WorkContextImplementation(ctx))
                    .As<WorkContext>()
                    .InstancePerMatchingLifetimeScope("work");
            });
            return new ThreadStaticScopeImplementation(
                workLifetime,
                EnsureThreadStaticContexts(),
                _workContextKey);
        }

        static ConcurrentDictionary<object, WorkContext> EnsureThreadStaticContexts() {
            return _threadStaticContexts ?? (_threadStaticContexts = new ConcurrentDictionary<object, WorkContext>());
        }

        private ILifetimeScope SpawnWorkLifetime(Action<ContainerBuilder> configurationAction) {
            return _lifetimeScope.BeginLifetimeScope("work", configurationAction);
        }


        class WorkContextImplementation : WorkContext {
            readonly IComponentContext _componentContext;
            readonly ConcurrentDictionary<string, object> _state = new ConcurrentDictionary<string, object>();
            readonly IEnumerable<IWorkContextStateProvider> _workContextStateProviders;

            public WorkContextImplementation(IComponentContext componentContext) {
                _componentContext = componentContext;
                _workContextStateProviders = componentContext.Resolve<IEnumerable<IWorkContextStateProvider>>();
            }

            public override T Resolve<T>() {
                return _componentContext.Resolve<T>();
            }

            public override T GetState<T>(string name) {
                return (T)_state.GetOrAdd(name, x => GetStateInternal<T>(x));
            }

            private T GetStateInternal<T>(string name) {
                return _workContextStateProviders.Select(wcsp => wcsp.Get<T>(name))
                    .FirstOrDefault(value => !Equals(value, default(T)));
            }

            public override void SetState<T>(string name, T value) {
                _state[name] = value;
            }
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
        }
    }


}

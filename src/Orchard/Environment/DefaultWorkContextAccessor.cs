using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        static readonly ConcurrentDictionary<object, WorkContext> _threadStaticContexts = new ConcurrentDictionary<object, WorkContext>();

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
            return _threadStaticContexts.TryGetValue(_workContextKey, out workContext) ? workContext : null;
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
                _threadStaticContexts,
                _workContextKey);
        }

        private ILifetimeScope SpawnWorkLifetime(Action<ContainerBuilder> configurationAction) {
            return _lifetimeScope.BeginLifetimeScope("work", configurationAction);
        }


        class WorkContextImplementation : WorkContext {
            private readonly IComponentContext _componentContext;

            public WorkContextImplementation(IComponentContext componentContext) {
                _componentContext = componentContext;
            }

            public override T Service<T>() {
                throw new NotImplementedException();
            }

            public override T State<T>() {
                throw new NotImplementedException();
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
                return WorkContext.Service<TService>();
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
                return WorkContext.Service<TService>();
            }
        }
    }


}

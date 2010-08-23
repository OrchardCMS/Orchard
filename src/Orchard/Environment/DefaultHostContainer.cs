using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Web;

namespace Orchard.Environment {
    //IContainerProvider

    public class DefaultOrchardHostContainer : IOrchardHostContainer, IMvcServiceLocator {
        private readonly IContainer _container;


        public DefaultOrchardHostContainer(IContainer container) {
            _container = container;
        }


        static bool TryResolveAtScope(ILifetimeScope scope, string key, Type serviceType, out object value) {
            if (scope == null) {
                value = null;
                return false;
            }
            return key == null ? scope.TryResolve(serviceType, out value) : scope.TryResolve(key, serviceType, out value);
        }

        bool TryResolve(string key, Type serviceType, out object value) {
            // shared objects are resolved from the host first
            // this is to ensure the lifecycle of components registered at the host level 
            // is consistent inside and outside of a containerproviderscope
            return
                TryResolveAtScope(_container, key, serviceType, out value) ||
                TryResolveAtScope(Scope.CurrentLifetimeScope, key, serviceType, out value);
        }

        static object CreateInstance(Type t) {
            if (t.IsAbstract || t.IsInterface)
                return null;

            return Activator.CreateInstance(t);
        }

        TService Resolve<TService>(Type serviceType, TService defaultValue = default(TService)) {
            object value;
            return TryResolve(null, serviceType, out value) ? (TService)value : defaultValue;
        }

        TService Resolve<TService>(Type serviceType, string key, TService defaultValue = default(TService)) {
            object value;
            return TryResolve(key, serviceType, out value) ? (TService)value : defaultValue;
        }

        TService Resolve<TService>(Type serviceType, Func<Type, TService> defaultFactory) {
            object value;
            return TryResolve(null, serviceType, out value) ? (TService)value : defaultFactory(serviceType);
        }

        TService Resolve<TService>(Type serviceType, string key, Func<Type, TService> defaultFactory) {
            object value;
            return TryResolve(key, serviceType, out value) ? (TService)value : defaultFactory(serviceType);
        }

        TService IOrchardHostContainer.Resolve<TService>() {
            // Resolve service, or null
            return Resolve(typeof(TService), default(TService));
        }

        object IServiceProvider.GetService(Type serviceType) {
            // Resolve service, or null
            return Resolve(serviceType, default(object));
        }

        object IServiceLocator.GetInstance(Type serviceType) {
            // Create instance, or default ctor
            return Resolve(serviceType, CreateInstance);
        }

        object IServiceLocator.GetInstance(Type serviceType, string key) {
            // Create instance, or default ctor
            return Resolve(serviceType, key, CreateInstance);
        }

        TService IServiceLocator.GetInstance<TService>() {
            // Create instance, or default ctor
            return Resolve(typeof(TService), t => (TService)CreateInstance(t));
        }

        TService IServiceLocator.GetInstance<TService>(string key) {
            // Create instance, or default ctor
            return Resolve(typeof(TService), key, t => (TService)CreateInstance(t));
        }

        IEnumerable<TService> IServiceLocator.GetAllInstances<TService>() {
            return Resolve(typeof(IEnumerable<TService>), Enumerable.Empty<TService>());
        }

        IEnumerable<object> IServiceLocator.GetAllInstances(Type serviceType) {
            return Resolve<IEnumerable>(typeof(IEnumerable<>).MakeGenericType(serviceType), Enumerable.Empty<object>()).Cast<object>();
        }

        void IMvcServiceLocator.Release(object instance) {
            // Autofac manages component disposal lifecycle internally.
        }


        public static IDisposable ContainerProviderScope(IContainerProvider containerProvider) {
            return new Scope(containerProvider);
        }

        class Scope : IDisposable {
            private readonly IContainerProvider _containerProvider;
            readonly Scope _prior;

            public Scope(IContainerProvider containerProvider) {
                _containerProvider = containerProvider;
                _prior = Current;
                Current = this;
            }

            public void Dispose() {
                Current = _prior;
            }

            public static ILifetimeScope CurrentLifetimeScope {
                get {
                    var currentScope = Current;
                    if (currentScope != null &&
                        currentScope._containerProvider != null) {
                        return currentScope._containerProvider.RequestLifetime;
                    }
                    return null;
                }
            }

            [ThreadStatic]
            static Scope _fallback;
            static readonly object _contextKey = new object();

            static Scope Current {
                get {
                    var context = HttpContext.Current;
                    return context != null ? (Scope)context.Items[_contextKey] : _fallback;
                }
                set {
                    var context = HttpContext.Current;
                    if (context != null)
                        context.Items[_contextKey] = value;
                    else {
                        _fallback = value;
                    }
                }
            }
        }
    }
}

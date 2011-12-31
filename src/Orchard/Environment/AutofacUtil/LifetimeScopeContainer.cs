using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;

namespace Orchard.Environment.AutofacUtil {
    public class LifetimeScopeContainer : IContainer {
        private readonly ILifetimeScope _lifetimeScope;

        public LifetimeScopeContainer(ILifetimeScope lifetimeScope) {
            _lifetimeScope = lifetimeScope;
        }

        public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters) {
            return _lifetimeScope.ResolveComponent(registration, parameters);
        }

        public IComponentRegistry ComponentRegistry {
            get { return _lifetimeScope.ComponentRegistry; }
        }

        public void Dispose() {
        }

        public ILifetimeScope BeginLifetimeScope() {
            return _lifetimeScope.BeginLifetimeScope();
        }

        public ILifetimeScope BeginLifetimeScope(object tag) {
            return _lifetimeScope.BeginLifetimeScope(tag);
        }

        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction) {
            return _lifetimeScope.BeginLifetimeScope(configurationAction);
        }

        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction) {
            return _lifetimeScope.BeginLifetimeScope(tag, configurationAction);
        }

        public IDisposer Disposer {
            get { return _lifetimeScope.Disposer; }
        }

        public object Tag {
            get { return _lifetimeScope.Tag; }
        }

        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;
        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;
        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;
    }
}
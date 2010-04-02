using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace Orchard.Environment.ShellBuilders {
    public class ContainerUpdater : ContainerBuilder {
        ICollection<Action<IComponentRegistry>> _configurationActions = new List<Action<IComponentRegistry>>();

        public override void RegisterCallback(Action<IComponentRegistry> configurationAction) {
            _configurationActions.Add(configurationAction);
        }

        public void Update(IContainer container) {
            foreach (var action in _configurationActions)
                action(container.ComponentRegistry);
        }

        public void Update(ILifetimeScope container) {
            foreach (var action in _configurationActions)
                action(container.ComponentRegistry);
        }
    }
}
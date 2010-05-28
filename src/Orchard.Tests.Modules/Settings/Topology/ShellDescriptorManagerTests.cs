using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Core.Settings.Topology;
using Orchard.Core.Settings.Topology.Records;
using Orchard.Environment.State;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Events;

namespace Orchard.Tests.Modules.Settings.Topology {
    [TestFixture]
    public class ShellDescriptorManagerTests : DatabaseEnabledTestsBase {
        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<ShellDescriptorManager>().As<IShellDescriptorManager>();
            builder.RegisterType<StubEventBus>().As<IEventBus>().SingleInstance();
            builder.RegisterSource(new EventsRegistrationSource());
        }

        public class StubEventBus : IEventBus {
            public string LastMessageName { get; set; }
            public IDictionary<string, object> LastEventData { get; set; }

            public void Notify_Obsolete(string messageName, IDictionary<string, string> eventData) {
            }

            public void Notify(string messageName, Dictionary<string, object> eventData) {                
                LastMessageName = messageName;
                LastEventData = eventData;
            }
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof (ShellDescriptorRecord),
                                 typeof (ShellFeatureRecord),
                                 typeof (ShellParameterRecord),
                             };
            }
        }

        [Test]
        public void TopologyShouldBeNullWhenItsNotInitialized() {
            var manager = _container.Resolve<IShellDescriptorManager>();
            var topology = manager.GetShellDescriptor();
            Assert.That(topology, Is.Null);
        }

        [Test]
        public void PriorSerialNumberOfZeroIsAcceptableForInitialUpdateAndSerialNumberIsNonzeroAfterwards() {
            var manager = _container.Resolve<IShellDescriptorManager>();
            manager.UpdateShellDescriptor(
                0,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            var topology = manager.GetShellDescriptor();
            Assert.That(topology, Is.Not.Null);
            Assert.That(topology.SerialNumber, Is.Not.EqualTo(0));
        }

        [Test]
        public void NonZeroInitialUpdateThrowsInvalidOperationException() {
            var manager = _container.Resolve<IShellDescriptorManager>();
            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                1,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>()));
        }

        [Test]
        public void OnlyCorrectSerialNumberOnLaterUpdatesDoesNotThrowException() {
            var manager = _container.Resolve<IShellDescriptorManager>();
            manager.UpdateShellDescriptor(
                0,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            var topology = manager.GetShellDescriptor();
            Assert.That(topology.SerialNumber, Is.Not.EqualTo(0));

            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                                               0,
                                               Enumerable.Empty<ShellFeature>(),
                                               Enumerable.Empty<ShellParameter>()));

            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                                               topology.SerialNumber + 665321,
                                               Enumerable.Empty<ShellFeature>(),
                                               Enumerable.Empty<ShellParameter>()));

            manager.UpdateShellDescriptor(
                topology.SerialNumber,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            var topology2 = manager.GetShellDescriptor();
            Assert.That(topology2.SerialNumber, Is.Not.EqualTo(0));
            Assert.That(topology2.SerialNumber, Is.Not.EqualTo(topology.SerialNumber));

            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                                               0,
                                               Enumerable.Empty<ShellFeature>(),
                                               Enumerable.Empty<ShellParameter>()));

            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                                               topology.SerialNumber,
                                               Enumerable.Empty<ShellFeature>(),
                                               Enumerable.Empty<ShellParameter>()));

            manager.UpdateShellDescriptor(
                topology2.SerialNumber,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                                               topology2.SerialNumber,
                                               Enumerable.Empty<ShellFeature>(),
                                               Enumerable.Empty<ShellParameter>()));
        }

        [Test]
        public void SuccessfulUpdateRaisesAnEvent() {
            var manager = _container.Resolve<IShellDescriptorManager>();
            var eventBus = _container.Resolve<IEventBus>() as StubEventBus;

            Assert.That(eventBus.LastMessageName, Is.Null);

            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                                               5,
                                               Enumerable.Empty<ShellFeature>(),
                                               Enumerable.Empty<ShellParameter>()));

            Assert.That(eventBus.LastMessageName, Is.Null);

            manager.UpdateShellDescriptor(
                0,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            Assert.That(eventBus.LastMessageName, Is.EqualTo("IShellDescriptorManagerEventHandler.Changed"));
        }

        [Test]
        public void ManagerReturnsStateForFeaturesInDescriptor() {
            var descriptorManager = _container.Resolve<IShellDescriptorManager>();
            var stateManager = _container.Resolve<IShellStateManager>();
            var state = stateManager.GetShellState();
            Assert.That(state.Features.Count(), Is.EqualTo(0));
            descriptorManager.UpdateShellDescriptor(
                0, 
                new[]{new ShellFeature{ Name="Foo"}},
                Enumerable.Empty<ShellParameter>());

            var state2 = stateManager.GetShellState();
            Assert.That(state2.Features.Count(), Is.EqualTo(1));
            Assert.That(state2.Features, Has.Some.Property("Name").EqualTo("Foo"));
        }
    }
}

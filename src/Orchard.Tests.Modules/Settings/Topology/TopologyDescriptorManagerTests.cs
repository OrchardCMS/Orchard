using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using NUnit.Framework;
using Orchard.Core.Settings.Topology;
using Orchard.Core.Settings.Topology.Records;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Events;

namespace Orchard.Tests.Modules.Settings.Topology {
    [TestFixture]
    public class TopologyDescriptorManagerTests : DatabaseEnabledTestsBase {
        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<TopologyDescriptorManager>().As<ITopologyDescriptorManager>();
            builder.RegisterType<StubEventBus>().As<IEventBus>().SingleInstance();

        }

        public class StubEventBus : IEventBus {
            public string LastMessageName { get; set; }
            public IDictionary<string, string> LastEventData { get; set; }

            public void Notify(string messageName, IDictionary<string, string> eventData) {
                LastMessageName = messageName;
                LastEventData = eventData;
            }
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof (TopologyRecord),
                                 typeof (TopologyFeatureRecord),
                                 typeof (TopologyParameterRecord),
                             };
            }
        }

        [Test]
        public void TopologyShouldBeNullWhenItsNotInitialized() {
            var manager = _container.Resolve<ITopologyDescriptorManager>();
            var topology = manager.GetTopologyDescriptor();
            Assert.That(topology, Is.Null);
        }

        [Test]
        public void PriorSerialNumberOfZeroIsAcceptableForInitialUpdateAndSerialNumberIsNonzeroAfterwards() {
            var manager = _container.Resolve<ITopologyDescriptorManager>();
            manager.UpdateTopologyDescriptor(
                0,
                Enumerable.Empty<TopologyFeature>(),
                Enumerable.Empty<TopologyParameter>());

            var topology = manager.GetTopologyDescriptor();
            Assert.That(topology, Is.Not.Null);
            Assert.That(topology.SerialNumber, Is.Not.EqualTo(0));
        }

        [Test]
        public void NonZeroInitialUpdateThrowsException() {
            var manager = _container.Resolve<ITopologyDescriptorManager>();
            Assert.Throws<Exception>(() => manager.UpdateTopologyDescriptor(
                1,
                Enumerable.Empty<TopologyFeature>(),
                Enumerable.Empty<TopologyParameter>()));
        }

        [Test]
        public void OnlyCorrectSerialNumberOnLaterUpdatesDoesNotThrowException() {
            var manager = _container.Resolve<ITopologyDescriptorManager>();
            manager.UpdateTopologyDescriptor(
                0,
                Enumerable.Empty<TopologyFeature>(),
                Enumerable.Empty<TopologyParameter>());

            var topology = manager.GetTopologyDescriptor();
            Assert.That(topology.SerialNumber, Is.Not.EqualTo(0));

            Assert.Throws<Exception>(() => manager.UpdateTopologyDescriptor(
                                               0,
                                               Enumerable.Empty<TopologyFeature>(),
                                               Enumerable.Empty<TopologyParameter>()));

            Assert.Throws<Exception>(() => manager.UpdateTopologyDescriptor(
                                               topology.SerialNumber + 665321,
                                               Enumerable.Empty<TopologyFeature>(),
                                               Enumerable.Empty<TopologyParameter>()));

            manager.UpdateTopologyDescriptor(
                topology.SerialNumber,
                Enumerable.Empty<TopologyFeature>(),
                Enumerable.Empty<TopologyParameter>());

            var topology2 = manager.GetTopologyDescriptor();
            Assert.That(topology2.SerialNumber, Is.Not.EqualTo(0));
            Assert.That(topology2.SerialNumber, Is.Not.EqualTo(topology.SerialNumber));

            Assert.Throws<Exception>(() => manager.UpdateTopologyDescriptor(
                                               0,
                                               Enumerable.Empty<TopologyFeature>(),
                                               Enumerable.Empty<TopologyParameter>()));

            Assert.Throws<Exception>(() => manager.UpdateTopologyDescriptor(
                                               topology.SerialNumber,
                                               Enumerable.Empty<TopologyFeature>(),
                                               Enumerable.Empty<TopologyParameter>()));

            manager.UpdateTopologyDescriptor(
                topology2.SerialNumber,
                Enumerable.Empty<TopologyFeature>(),
                Enumerable.Empty<TopologyParameter>());

            Assert.Throws<Exception>(() => manager.UpdateTopologyDescriptor(
                                               topology2.SerialNumber,
                                               Enumerable.Empty<TopologyFeature>(),
                                               Enumerable.Empty<TopologyParameter>()));
        }

        [Test]
        public void SuccessfulUpdateRaisesAnEvent() {
            var manager = _container.Resolve<ITopologyDescriptorManager>();
            var eventBus = _container.Resolve<IEventBus>() as StubEventBus;

            Assert.That(eventBus.LastMessageName, Is.Null);

            Assert.Throws<Exception>(() => manager.UpdateTopologyDescriptor(
                                               5,
                                               Enumerable.Empty<TopologyFeature>(),
                                               Enumerable.Empty<TopologyParameter>()));

            Assert.That(eventBus.LastMessageName, Is.Null);

            manager.UpdateTopologyDescriptor(
                0,
                Enumerable.Empty<TopologyFeature>(),
                Enumerable.Empty<TopologyParameter>());

            Assert.That(eventBus.LastMessageName, Is.EqualTo(typeof(ITopologyDescriptorManager).FullName + ".UpdateTopologyDescriptor"));
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Core.Settings.State;
using Orchard.Core.Settings.Descriptor;
using Orchard.Core.Settings.Descriptor.Records;
using Orchard.Environment.Configuration;
using Orchard.Environment.State;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Events;

namespace Orchard.Tests.Modules.Settings.Blueprint {
    [TestFixture]
    public class ShellDescriptorManagerTests : DatabaseEnabledTestsBase {
        public override void Register(ContainerBuilder builder) {
            builder.RegisterInstance(new ShellSettings { Name = "Default" });

            builder.RegisterType<ShellDescriptorManager>().As<IShellDescriptorManager>().SingleInstance();
            builder.RegisterType<ShellStateManager>().As<IShellStateManager>().SingleInstance();
            builder.RegisterType<StubEventBus>().As<IEventBus>().SingleInstance();
            builder.RegisterSource(new EventsRegistrationSource());
        }

        public class StubEventBus : IEventBus {
            public string LastMessageName { get; set; }
            public IDictionary<string, object> LastEventData { get; set; }

            public IEnumerable Notify(string messageName, IDictionary<string, object> eventData) {
                LastMessageName = messageName;
                LastEventData = eventData;
                return new object[0];
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
        public void BlueprintShouldBeNullWhenItsNotInitialized() {
            var manager = _container.Resolve<IShellDescriptorManager>();
            var descriptor = manager.GetShellDescriptor();
            Assert.That(descriptor, Is.Null);
        }

        [Test]
        public void PriorSerialNumberOfZeroIsAcceptableForInitialUpdateAndSerialNumberIsNonzeroAfterwards() {
            var manager = _container.Resolve<IShellDescriptorManager>();
            manager.UpdateShellDescriptor(
                0,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            var descriptor = manager.GetShellDescriptor();
            Assert.That(descriptor, Is.Not.Null);
            Assert.That(descriptor.SerialNumber, Is.Not.EqualTo(0));
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

            var descriptor = manager.GetShellDescriptor();
            Assert.That(descriptor.SerialNumber, Is.Not.EqualTo(0));

            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                                               0,
                                               Enumerable.Empty<ShellFeature>(),
                                               Enumerable.Empty<ShellParameter>()));

            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                                               descriptor.SerialNumber + 665321,
                                               Enumerable.Empty<ShellFeature>(),
                                               Enumerable.Empty<ShellParameter>()));

            manager.UpdateShellDescriptor(
                descriptor.SerialNumber,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            var descriptor2 = manager.GetShellDescriptor();
            Assert.That(descriptor2.SerialNumber, Is.Not.EqualTo(0));
            Assert.That(descriptor2.SerialNumber, Is.Not.EqualTo(descriptor.SerialNumber));

            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                                               0,
                                               Enumerable.Empty<ShellFeature>(),
                                               Enumerable.Empty<ShellParameter>()));

            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                                               descriptor.SerialNumber,
                                               Enumerable.Empty<ShellFeature>(),
                                               Enumerable.Empty<ShellParameter>()));

            manager.UpdateShellDescriptor(
                descriptor2.SerialNumber,
                Enumerable.Empty<ShellFeature>(),
                Enumerable.Empty<ShellParameter>());

            Assert.Throws<InvalidOperationException>(() => manager.UpdateShellDescriptor(
                                               descriptor2.SerialNumber,
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

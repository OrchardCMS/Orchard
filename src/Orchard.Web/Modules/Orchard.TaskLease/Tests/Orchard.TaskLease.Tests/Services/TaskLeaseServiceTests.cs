using System;
using System.Collections.Generic;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.TaskLease.Models;
using Orchard.TaskLease.Services;
using Orchard.Tests.Modules;

namespace Orchard.TaskLease.Tests.Services
{
    [TestFixture]
    public class TaskLeaseServiceTests : DatabaseEnabledTestsBase
    {
        private ITaskLeaseService _service;
        private Mock<IMachineNameProvider> _machineNameProvider;

        protected override IEnumerable<Type> DatabaseTypes
        {
            get
            {
                return new[] {
                    typeof (TaskLeaseRecord)
                };
            }
        }

        public override void Register(ContainerBuilder builder)
        {
            builder.RegisterType<TaskLeaseService>().As<ITaskLeaseService>();
            builder.RegisterInstance((_machineNameProvider = new Mock<IMachineNameProvider>()).Object);

            _machineNameProvider.Setup(x => x.GetMachineName()).Returns("SkyNet");
        }

        public override void Init()
        {
            base.Init();
            _service = _container.Resolve<ITaskLeaseService>();
        }

        [Test]
        public void AcquireShouldSucceedIfNoTask()
        {
            var state = _service.Acquire("Foo", _clock.UtcNow.AddDays(1));
            Assert.That(state, Is.EqualTo(String.Empty));
        }

        [Test]
        public void AcquireShouldSucceedIfTaskBySameMachine()
        {
            var state = _service.Acquire("Foo", _clock.UtcNow.AddDays(1));
            Assert.That(state, Is.EqualTo(String.Empty));

            state = _service.Acquire("Foo", _clock.UtcNow.AddDays(1));
            Assert.That(state, Is.EqualTo(String.Empty));
        }

        [Test]
        public void AcquireShouldNotSucceedIfTaskByOtherMachine()
        {
            var state = _service.Acquire("Foo", _clock.UtcNow.AddDays(1));
            Assert.That(state, Is.EqualTo(String.Empty));

            _machineNameProvider.Setup(x => x.GetMachineName()).Returns("TheMatrix");

            state = _service.Acquire("Foo", _clock.UtcNow.AddDays(1));
            Assert.That(state, Is.EqualTo(null));
        }

        [Test]
        public void ShouldUpdateTask()
        {
            var state = _service.Acquire("Foo", _clock.UtcNow.AddDays(1));
            Assert.That(state, Is.EqualTo(String.Empty));

            _service.Update("Foo", "Other");
            state = _service.Acquire("Foo", _clock.UtcNow.AddDays(1));

            Assert.That(state, Is.EqualTo("Other"));
        }

        [Test]
        public void AcquireShouldSucceedIfTaskByOtherMachineAndExpired()
        {
            var state = _service.Acquire("Foo", _clock.UtcNow.AddDays(1));
            Assert.That(state, Is.EqualTo(String.Empty));

            _machineNameProvider.Setup(x => x.GetMachineName()).Returns("TheMatrix");
            _clock.Advance(new TimeSpan(2,0,0,0));

            state = _service.Acquire("Foo", _clock.UtcNow.AddDays(1));
            Assert.That(state, Is.EqualTo(String.Empty));

        }
    }
}
using System;
using System.Collections.Generic;
using Autofac.Builder;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Scheduling;
using Orchard.Core.Scheduling.Models;
using Orchard.Core.Scheduling.Services;
using Orchard.Data;
using Orchard.Services;
using Orchard.Tasks;
using Orchard.Tests.Packages;
using Orchard.Tests.Stubs;

namespace Orchard.Core.Tests.Scheduling {
    [TestFixture]
    public class ScheduledTaskExecutorTests : DatabaseEnabledTestsBase {
        private StubTaskHandler _handler;
        private IBackgroundTask _executor;
        private IRepository<ScheduledTaskRecord> _repository;

        public override void Init() {
            base.Init();
            _repository = _container.Resolve<IRepository<ScheduledTaskRecord>>();
            _executor = _container.Resolve<IBackgroundTask>("ScheduledBackgroundTask");
        }
        public override void Register(ContainerBuilder builder) {
            _handler = new StubTaskHandler();
            builder.Register(new Mock<IOrchardServices>().Object);
            builder.Register<DefaultContentManager>().As<IContentManager>();
            builder.Register<SchedulingBackgroundTask>().As<IBackgroundTask>().Named("ScheduledBackgroundTask");
            builder.Register(_handler).As<IScheduledTaskHandler>();
        }

        public class StubTaskHandler : IScheduledTaskHandler {
            public void Process(ScheduledTaskContext context) {
                TaskContext = context;
            }

            public ScheduledTaskContext TaskContext { get; private set; }
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof(ContentTypeRecord), 
                                 typeof(ContentItemRecord), 
                                 typeof(ContentItemVersionRecord), 
                                 typeof(ScheduledAspectRecord),
                                 typeof(ScheduledTaskRecord),
                             };
            }
        }

        [Test]
        public void SweepShouldBeCallable() {
            _executor.Sweep();
        }

        [Test]
        public void RecordsForTheFutureShouldBeIgnored() {
            _repository.Create(new ScheduledTaskRecord { ScheduledUtc = _clock.UtcNow.Add(TimeSpan.FromHours(2)) });
            _repository.Flush();
            _executor.Sweep();
            _repository.Flush();
            Assert.That(_repository.Count(x => true), Is.EqualTo(1));
        }


        [Test]
        public void RecordsWhenTheyAreExecutedShouldBeDeleted() {
            var task = new ScheduledTaskRecord { Action = "Ignore", ScheduledUtc = _clock.UtcNow.Add(TimeSpan.FromHours(2)) };
            _repository.Create(task);

            _repository.Flush();
            _executor.Sweep();

            _repository.Flush();
            Assert.That(_repository.Count(x => x.Action == "Ignore"), Is.EqualTo(1));

            _clock.Advance(TimeSpan.FromHours(3));

            _repository.Flush();
            _executor.Sweep();

            _repository.Flush();
            Assert.That(_repository.Count(x => x.Action == "Ignore"), Is.EqualTo(0));
        }

        [Test]
        public void ScheduledTaskHandlersShouldBeCalledWhenTasksAreExecuted() {
            var task = new ScheduledTaskRecord { Action = "Ignore", ScheduledUtc = _clock.UtcNow.Add(TimeSpan.FromHours(2)) };
            _repository.Create(task);

            _repository.Flush();
            _clock.Advance(TimeSpan.FromHours(3));

            Assert.That(_handler.TaskContext, Is.Null);
            _executor.Sweep();
            Assert.That(_handler.TaskContext, Is.Not.Null);
            
            Assert.That(_handler.TaskContext.ScheduledTaskRecord.Action, Is.EqualTo("Ignore"));
            Assert.That(_handler.TaskContext.ContentItem, Is.Null);
        }
    }
}


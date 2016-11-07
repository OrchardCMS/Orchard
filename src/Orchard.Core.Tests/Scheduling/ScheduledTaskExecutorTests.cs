using System;
using System.Collections.Generic;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Scheduling.Models;
using Orchard.Core.Scheduling.Services;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.Tasks;
using Orchard.Tasks.Scheduling;
using Orchard.Tests.Modules;
using Orchard.Tests.Stubs;
using Orchard.UI.PageClass;

namespace Orchard.Core.Tests.Scheduling {
    [TestFixture]
    public class ScheduledTaskExecutorTests : DatabaseEnabledTestsBase {
        private StubTaskHandler _handler;
        private IBackgroundTask _executor;
        private IRepository<ScheduledTaskRecord> _repository;

        public override void Init() {
            base.Init();
            _repository = _container.Resolve<IRepository<ScheduledTaskRecord>>();
            _executor = _container.ResolveNamed<IBackgroundTask>("ScheduledTaskExecutor");
        }
        public override void Register(ContainerBuilder builder) {
            _handler = new StubTaskHandler();
            builder.RegisterInstance(new Mock<IOrchardServices>().Object);
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<ShapeTableLocator>().As<IShapeTableLocator>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterInstance(new Mock<IContentDefinitionManager>().Object);
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);

            builder.RegisterType<ScheduledTaskExecutor>().As<IBackgroundTask>().Named("ScheduledTaskExecutor", typeof(IBackgroundTask));
            builder.RegisterInstance(_handler).As<IScheduledTaskHandler>();

            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();
            builder.RegisterInstance(new Mock<IPageClassBuilder>().Object); 
            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof(ContentTypeRecord), 
                                 typeof(ContentItemRecord), 
                                 typeof(ContentItemVersionRecord), 
                                 typeof(ScheduledTaskRecord),
                             };
            }
        }

        public class StubTaskHandler : IScheduledTaskHandler {
            public void Process(ScheduledTaskContext context) {
                TaskContext = context;
            }

            public ScheduledTaskContext TaskContext { get; private set; }
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
            Assert.That(_repository.Count(x => x != null), Is.EqualTo(1));
        }


        [Test]
        public void RecordsWhenTheyAreExecutedShouldBeDeleted() {
            var task = new ScheduledTaskRecord { TaskType = "Ignore", ScheduledUtc = _clock.UtcNow.Add(TimeSpan.FromHours(2)) };
            _repository.Create(task);

            _repository.Flush();
            _executor.Sweep();

            _repository.Flush();
            Assert.That(_repository.Count(x => x.TaskType == "Ignore"), Is.EqualTo(1));

            _clock.Advance(TimeSpan.FromHours(3));

            _repository.Flush();
            _executor.Sweep();

            _repository.Flush();
            Assert.That(_repository.Count(x => x.TaskType == "Ignore"), Is.EqualTo(0));
        }

        [Test]
        public void ScheduledTaskHandlersShouldBeCalledWhenTasksAreExecuted() {
            var task = new ScheduledTaskRecord { TaskType = "Ignore", ScheduledUtc = _clock.UtcNow.Add(TimeSpan.FromHours(2)) };
            _repository.Create(task);

            _repository.Flush();
            _clock.Advance(TimeSpan.FromHours(3));

            Assert.That(_handler.TaskContext, Is.Null);
            _executor.Sweep();
            Assert.That(_handler.TaskContext, Is.Not.Null);

            Assert.That(_handler.TaskContext.Task.TaskType, Is.EqualTo("Ignore"));
            Assert.That(_handler.TaskContext.Task.ContentItem, Is.Null);
        }
    }
}


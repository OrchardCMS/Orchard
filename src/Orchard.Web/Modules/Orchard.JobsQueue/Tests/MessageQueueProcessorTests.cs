using System;
using System.Collections.Generic;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.JobsQueue.Models;
using Orchard.JobsQueue.Services;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.Tests;

namespace Orchard.Messaging.Tests {
    [TestFixture]
    public class MessageQueueProcessorTests : DatabaseEnabledTestsBase {
        private List<QueuedJobRecord> _messages;

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                yield return typeof(QueuedJobRecord);
            }
        }

        public override void Register(ContainerBuilder builder) {
            var messageManagerMock = new Mock<IJobsQueueService>();

            builder.RegisterInstance(messageManagerMock.Object);
            builder.RegisterType<JobsQueueProcessor>().As<IJobsQueueProcessor>();
            builder.RegisterType<StubMessageChannel>().As<IMessageChannel>();

            _messages = new List<QueuedJobRecord> {
                CreateMessage("Message 1"),
                CreateMessage("Message 2")
            };

            messageManagerMock
                .Setup(x => x.Enqueue(It.IsAny<string>(), It.IsAny<string>(), 0))
                .Callback(() => _clock.Advance(TimeSpan.FromSeconds(1)))
                .Returns(new QueuedJobRecord ());
            //messageManagerMock.Setup(x => x.EnterProcessingStatus()).Callback(() => {
            //    queue.Record.Status = MessageQueueStatus.Processing;
            //    queue.Record.StartedUtc = _clock.UtcNow;
            //});
        }

        [Test]
        public void ProcessingQueueWithEnoughTimeSendsAllMessages() {
            var processor = _container.Resolve<IJobsQueueProcessor>();
            
            processor.ProcessQueue();
            
        }

        private QueuedJobRecord CreateMessage(string subject) {
            return new QueuedJobRecord {Id = 1, Message = "Email", Parameters = "some payload data"};
        }
    }
}
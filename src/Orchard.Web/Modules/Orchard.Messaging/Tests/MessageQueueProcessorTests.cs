using System;
using System.Collections.Generic;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.Tests;

namespace Orchard.Messaging.Tests {
    [TestFixture]
    public class MessageQueueProcessorTests : DatabaseEnabledTestsBase {
        private List<QueuedMessageRecord> _messages;

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                yield return typeof(QueuedMessageRecord);
            }
        }

        public override void Register(ContainerBuilder builder) {
            var messageManagerMock = new Mock<IMessageQueueService>();

            builder.RegisterInstance(messageManagerMock.Object);
            builder.RegisterType<MessageQueueProcessor>().As<IMessageQueueProcessor>();
            builder.RegisterType<StubMessageChannel>().As<IMessageChannel>();

            _messages = new List<QueuedMessageRecord> {
                CreateMessage("Message 1"),
                CreateMessage("Message 2")
            };

            messageManagerMock
                .Setup(x => x.Enqueue(It.IsAny<string>(), It.IsAny<string>(), 0))
                .Callback(() => _clock.Advance(TimeSpan.FromSeconds(1)))
                .Returns(new QueuedMessageRecord ());
            //messageManagerMock.Setup(x => x.EnterProcessingStatus()).Callback(() => {
            //    queue.Record.Status = MessageQueueStatus.Processing;
            //    queue.Record.StartedUtc = _clock.UtcNow;
            //});
        }

        [Test]
        public void ProcessingQueueWithEnoughTimeSendsAllMessages() {
            var processor = _container.Resolve<IMessageQueueProcessor>();
            
            processor.ProcessQueue();

            foreach (var message in _messages) {
                Assert.That(message.Status, Is.EqualTo(QueuedMessageStatus.Sent));
            }
        }

        private QueuedMessageRecord CreateMessage(string subject) {
            return new QueuedMessageRecord {Id = 1, Type = "Email", Payload = "some payload data"};
        }
    }
}
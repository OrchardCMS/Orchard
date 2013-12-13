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
        private List<QueuedMessage> _messages;

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                yield return typeof(MessageQueueRecord);
                yield return typeof(MessagePriority);
                yield return typeof(QueuedMessageRecord);
            }
        }

        public override void Register(ContainerBuilder builder) {
            var messageManagerMock = new Mock<IMessageQueueManager>();
            var queue = new MessageQueue(new MessageQueueRecord {
                Name = "Default"
            });

            builder.RegisterInstance(messageManagerMock.Object);
            builder.RegisterType<MessageQueueProcessor>().As<IMessageQueueProcessor>();
            builder.RegisterType<StubMessageChannel>().As<IMessageChannel>();

            var queues = new List<MessageQueue> {queue};
            _messages = new List<QueuedMessage> {
                CreateMessage("Message 1"),
                CreateMessage("Message 2")
            };

            messageManagerMock
                .Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessagePriority>(), null))
                .Callback(() => _clock.Advance(TimeSpan.FromSeconds(1)))
                .Returns(new QueuedMessage(new QueuedMessageRecord ()));
            messageManagerMock.Setup(x => x.GetIdleQueues()).Returns(queues);
            messageManagerMock.Setup(x => x.EnterProcessingStatus(queue)).Callback(() => {
                queue.Record.Status = MessageQueueStatus.Processing;
                queue.Record.StartedUtc = _clock.UtcNow;
            });
        }

        [Test]
        public void ProcessingQueueWithEnoughTimeSendsAllMessages() {
            var processor = _container.Resolve<IMessageQueueProcessor>();
            
            processor.ProcessQueues();

            foreach (var message in _messages) {
                Assert.That(message.Status, Is.EqualTo(QueuedMessageStatus.Sent));
            }
        }

        private QueuedMessage CreateMessage(string subject) {
            return new QueuedMessage(new QueuedMessageRecord {Id = 1,  Payload = "some payload data"}) {
                ChannelField = new Lazy<IMessageChannel>(() => _container.Resolve<IMessageChannel>(new NamedParameter("simulatedProcessingTime", TimeSpan.FromSeconds(1)), new NamedParameter("clock", _clock))),
                RecipientsField = new Lazy<IEnumerable<MessageRecipient>>(() => new[]{ new MessageRecipient("recipient@domain.com") })
            };
        }
    }
}
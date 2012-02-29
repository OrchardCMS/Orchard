using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement.Records;
using Orchard.Messaging.Events;
using Orchard.Messaging.Services;
using Orchard.Tests.Messaging;
using Orchard.Tests.Utility;

namespace Orchard.Tests.Modules.Email {
    [TestFixture]
    public class EmailChannelTests {
        private MessagingChannelStub _channel;
        private IMessageManager _messageManager;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new Mock<IMessageEventHandler>().Object);
            builder.RegisterType<DefaultMessageManager>().As<IMessageManager>();
            builder.RegisterInstance(_channel = new MessagingChannelStub()).As<IMessagingChannel>();

            var container = builder.Build();
            _messageManager = container.Resolve<IMessageManager>();
        }
    
        [Test]
        public void CanSendEmailUsingAddresses() {
            _messageManager.Send(new []{ "steveb@microsoft.com" }, "test", "email");
            Assert.That(_channel.Messages.Count, Is.EqualTo(1));
        }

        [Test]
        public void OneMessageIsSentUsingMultipleRecipients() {
            _messageManager.Send(new[] { "steveb@microsoft.com", "billg@microsoft.com" }, "test", "email");
            Assert.That(_channel.Messages.Count, Is.EqualTo(1));
        }
    }
}
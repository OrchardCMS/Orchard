using System.Collections.Generic;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement.Records;
using Orchard.DisplayManagement;
using Orchard.Email.Services;
using Orchard.Messaging.Events;
using Orchard.Messaging.Services;
using Orchard.Tests.Messaging;
using Orchard.Tests.Modules.Stubs;
using Orchard.Tests.Utility;

namespace Orchard.Tests.Modules.Email {
    [TestFixture]
    public class EmailChannelTests {
        private IMessageService _messageService;
        private SmtpChannelStub _smtpChannel;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            _smtpChannel = new SmtpChannelStub();

            builder.RegisterType<DefaultMessageService>().As<IMessageService>();
            builder.RegisterType<MessageChannelManager>().As<IMessageChannelManager>();
            builder.RegisterType<ShapeDisplayStub>().As<IShapeDisplay>();
            builder.RegisterInstance(new MessageChannelSelectorStub(_smtpChannel)).As<IMessageChannelSelector>();

            var container = builder.Build();
            _messageService = container.Resolve<IMessageService>();
        }
    
        [Test]
        public void CanSendEmailUsingAddresses() {
            _messageService.Send("Email", new Dictionary<string, object>() { {"", null} });
            Assert.That(_smtpChannel.Processed, Is.Not.Null);
        }

    }

    public class SmtpChannelStub : IMessageChannel {
        public IDictionary<string, object> Processed;
        public void Process(IDictionary<string, object> parameters) {
            Processed = parameters;
        }
    }
}
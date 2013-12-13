using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.Tests;

namespace Orchard.Messaging.Tests {
    [TestFixture]
    public class MessageQueueTests : DatabaseEnabledTestsBase {
        protected override IEnumerable<Type> DatabaseTypes {
            get {
                yield return typeof(MessageQueueRecord);
                yield return typeof(MessagePriority);
                yield return typeof(QueuedMessageRecord);
            }
        }

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<MessageQueueManager>().As<IMessageQueueManager>();
        }

        [Test]
        public void SendingWithoutSpecifyingQueueUsesDefaultQueue() {
            var manager = _container.Resolve<IMessageQueueManager>();
            var createdMessage = manager.Send("john.doe@live.com", "email", "This is a test subject");
            var defaultQueue = manager.GetDefaultQueue();
            Assert.That(createdMessage.Queue.Id, Is.EqualTo(defaultQueue.Id));
        }

        [Test]
        public void SendingWithoutExistingPrioritiesCreatesDefaultPriorities() {
            var manager = _container.Resolve<IMessageQueueManager>();
            var createdMessage = manager.Send("john.doe@live.com", "email", "This is a test subject");
            var low = manager.GetPriority("Low");
            var normal = manager.GetPriority("Normal");
            var high = manager.GetPriority("High");
            Assert.That(low, Is.Not.Null);
            Assert.That(normal, Is.Not.Null);
            Assert.That(high, Is.Not.Null);
        }

        [Test]
        public void SendingWithoutExplicitPriorityUsesDefaultPriority() {
            var manager = _container.Resolve<IMessageQueueManager>();
            var createdMessage = manager.Send("john.doe@live.com", "email", "This is a test subject");
            var defaultPriority = manager.GetDefaultPriority();
            Assert.That(createdMessage.Priority.Id, Is.EqualTo(defaultPriority.Id));
        }

        [Test]
        public void DefaultPriorityIsLowestPriority() {
            var manager = _container.Resolve<IMessageQueueManager>();
            var priorities = manager.CreateDefaultPriorities().ToList();
            var expectedPriority = priorities.OrderByDescending(x => x.Value).First();
            var actualPriority = manager.GetDefaultPriority();
            Assert.That(expectedPriority.Id, Is.EqualTo(actualPriority.Id));
        }
    }
}
using System;
using System.Collections.Generic;
using Autofac;
using NUnit.Framework;
using Orchard.JobsQueue.Models;
using Orchard.JobsQueue.Services;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.Tests;

namespace Orchard.Messaging.Tests {
    [TestFixture]
    public class MessageQueueTests : DatabaseEnabledTestsBase {
        protected override IEnumerable<Type> DatabaseTypes {
            get {
                yield return typeof(QueuedJobRecord);
            }
        }

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<JobsQueueService>().As<IJobsQueueService>();
        }

    }
}
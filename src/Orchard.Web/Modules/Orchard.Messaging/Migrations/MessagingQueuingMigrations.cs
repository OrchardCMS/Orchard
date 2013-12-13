using System;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Messaging.Services;

namespace Orchard.Messaging.Migrations {
    [OrchardFeature("Orchard.Messaging.Queuing")]
    public class MessagingQueuingMigrations : DataMigrationImpl {
        private readonly IMessageQueueManager _messageQueueManager;

        public MessagingQueuingMigrations(IMessageQueueManager messageQueueManager) {
            _messageQueueManager = messageQueueManager;
        }

        public int Create() {
            SchemaBuilder.CreateTable("MessagePriority", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<int>("Value", c => c.NotNull())
                .Column<string>("Name", c => c.WithLength(50))
                .Column<string>("DisplayText", c => c.WithLength(50))
                .Column<bool>("Archived", c => c.NotNull())
                .Column<DateTime>("ArchivedUtc"));

            SchemaBuilder.CreateTable("MessageQueueRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Name", c => c.WithLength(50))
                .Column<string>("Status", c => c.WithLength(50))
                .Column<DateTime>("StartedUtc")
                .Column<DateTime>("EndedUtc"));

            SchemaBuilder.CreateTable("QueuedMessageRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<int>("QueueId", c => c.NotNull())
                .Column<int>("Priority_Id")
                .Column<string>("ChannelName", c => c.WithLength(50))
                .Column<string>("Recipients", c => c.Unlimited())
                .Column<string>("Payload", c => c.Unlimited())
                .Column<string>("Status", c => c.WithLength(50))
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("StartedUtc")
                .Column<DateTime>("CompletedUtc")
                .Column<string>("Result", c => c.Unlimited()));

            _messageQueueManager.CreateDefaultQueue();
            _messageQueueManager.CreateDefaultPriorities();

            return 1;
        }
    }
}
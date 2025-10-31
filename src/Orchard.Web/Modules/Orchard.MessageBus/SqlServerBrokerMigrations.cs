using System;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Orchard.MessageBus
{
    [OrchardFeature("Orchard.MessageBus.SqlServerServiceBroker")]
    public class SqlServerBrokerMigrations : DataMigrationImpl
    {

        public int Create()
        {

            SchemaBuilder.CreateTable("MessageRecord",
                table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<string>("Publisher", c => c.WithLength(255))
                    .Column<string>("Channel", c => c.WithLength(255))
                    .Column<string>("Message", c => c.Unlimited())
                    .Column<DateTime>("CreatedUtc")
            );

            return 1;
        }
    }
}
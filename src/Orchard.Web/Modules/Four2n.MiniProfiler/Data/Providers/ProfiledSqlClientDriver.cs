// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProfiledSqlClientDriver.cs" company="Daniel Dabrowski - rod.42n.pl">
//   Copyright (c) 2008 Daniel Dabrowski - 42n. All rights reserved.
// </copyright>
// <summary>
//   Defines the ProfiledSqlClientDriver type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Four2n.Orchard.MiniProfiler.Data.Providers
{
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;

    using NHibernate.Driver;

    using StackExchange.Profiling.Data;

    public class ProfiledSqlClientDriver : SqlClientDriver
    {
        public override IDbCommand CreateCommand()
        {
            var command = base.CreateCommand();
            if (StackExchange.Profiling.MiniProfiler.Current != null)
            {
                command = new ProfiledDbCommand(
                    (DbCommand)command,
                    (ProfiledDbConnection)command.Connection,
                    StackExchange.Profiling.MiniProfiler.Current);
            }

            return command;
        }

        public override IDbConnection CreateConnection()
        {
            if (StackExchange.Profiling.MiniProfiler.Current == null)
            {
                return base.CreateConnection();
            }

            return new ProfiledDbConnection(
                base.CreateConnection() as DbConnection,
                StackExchange.Profiling.MiniProfiler.Current);
        }
    }
}
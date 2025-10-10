using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Glimpse.Ado.AlternateType;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace Orchard.Glimpse.ADO {
    public class GlimpseDriver : IDriver {
        private readonly IDriver _decoratedService;

        public GlimpseDriver(IDriver decoratedService) {
            _decoratedService = decoratedService;
        }

        public void Configure(IDictionary<string, string> settings) {
            _decoratedService.Configure(settings);
        }

        public DbConnection CreateConnection() {
            return new GlimpseDbConnection(_decoratedService.CreateConnection() as DbConnection);
        }

        public DbCommand GenerateCommand(CommandType type, SqlString sqlString, SqlType[] parameterTypes) {
            return new GlimpseDbCommand(_decoratedService.GenerateCommand(type, sqlString, parameterTypes) as DbCommand);
        }

        public void PrepareCommand(DbCommand command) {
            _decoratedService.PrepareCommand(command);
        }

        public DbParameter GenerateParameter(DbCommand command, string name, SqlType sqlType) {
            return _decoratedService.GenerateParameter(command, name, sqlType);
        }

        public void RemoveUnusedCommandParameters(DbCommand cmd, SqlString sqlString) {
            _decoratedService.RemoveUnusedCommandParameters(cmd, sqlString);
        }

        public void ExpandQueryParameters(DbCommand cmd, SqlString sqlString, SqlType[] parameterTypes) {
            _decoratedService.ExpandQueryParameters(cmd, sqlString, parameterTypes);
        }

        public IResultSetsCommand GetResultSetsCommand(ISessionImplementor session) {
            return _decoratedService.GetResultSetsCommand(session);
        }

        public void AdjustCommand(DbCommand command) {
            _decoratedService.AdjustCommand(command);
        }

        public bool SupportsMultipleOpenReaders => _decoratedService.SupportsMultipleOpenReaders;

        public bool SupportsMultipleQueries => _decoratedService.SupportsMultipleQueries;

        public bool RequiresTimeSpanForTime => _decoratedService.RequiresTimeSpanForTime;

        public bool SupportsSystemTransactions => _decoratedService.SupportsSystemTransactions;

        public bool SupportsNullEnlistment => _decoratedService.SupportsNullEnlistment;

        public bool SupportsEnlistmentWhenAutoEnlistmentIsDisabled => _decoratedService.SupportsEnlistmentWhenAutoEnlistmentIsDisabled;

        public bool HasDelayedDistributedTransactionCompletion => _decoratedService.HasDelayedDistributedTransactionCompletion;

        public DateTime MinDate => _decoratedService.MinDate;
    }
}
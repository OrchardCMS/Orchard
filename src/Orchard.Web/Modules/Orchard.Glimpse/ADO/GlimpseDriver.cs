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

        public IDbConnection CreateConnection() {
            return new GlimpseDbConnection(_decoratedService.CreateConnection() as DbConnection);
        }

        public IDbCommand GenerateCommand(CommandType type, SqlString sqlString, SqlType[] parameterTypes) {
            return new GlimpseDbCommand(_decoratedService.GenerateCommand(type, sqlString, parameterTypes) as DbCommand);
        }

        public void PrepareCommand(IDbCommand command) {
            _decoratedService.PrepareCommand(command);
        }

        public IDbDataParameter GenerateParameter(IDbCommand command, string name, SqlType sqlType) {
            return _decoratedService.GenerateParameter(command, name, sqlType);
        }

        public void RemoveUnusedCommandParameters(IDbCommand cmd, SqlString sqlString) {
            _decoratedService.RemoveUnusedCommandParameters(cmd, sqlString);
        }

        public void ExpandQueryParameters(IDbCommand cmd, SqlString sqlString) {
            _decoratedService.ExpandQueryParameters(cmd, sqlString);
        }

        public IResultSetsCommand GetResultSetsCommand(ISessionImplementor session) {
            return _decoratedService.GetResultSetsCommand(session);
        }

        public void AdjustCommand(IDbCommand command) {
            _decoratedService.AdjustCommand(command);
        }

        public bool SupportsMultipleOpenReaders => _decoratedService.SupportsMultipleOpenReaders;

        public bool SupportsMultipleQueries => _decoratedService.SupportsMultipleQueries;
    }
}
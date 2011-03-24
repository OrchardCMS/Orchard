using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Mvc.Filters;

namespace Orchard.Experimental {
    [OrchardFeature("Orchard.Experimental.LockLogging")]
    public class LockLogging : FilterProvider, IExceptionFilter {
        private readonly Work<ISessionLocator> _sessionLocator;

        public LockLogging(Work<ISessionLocator> sessionLocator) {
            _sessionLocator = sessionLocator;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void OnException(ExceptionContext filterContext) {
            var connection = _sessionLocator.Value.For(null).Connection;
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM sys.lock_information";
            var reader = command.ExecuteReader();
            while (reader.Read()) {
                var fields = Enumerable.Range(0, reader.FieldCount)
                    .Select(i => new { Key = reader.GetName(i), Value = reader.GetValue(i) });
                var message = fields.Aggregate("sys.lock_information", (sz, kv) => sz + " " + kv.Key + ":" + kv.Value);
                Logger.Debug(message);
            }
        }
    }
}
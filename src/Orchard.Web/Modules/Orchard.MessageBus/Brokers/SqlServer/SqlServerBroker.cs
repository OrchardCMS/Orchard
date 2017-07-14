using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Web;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.MessageBus.Models;
using Orchard.MessageBus.Services;
using Orchard.Services;

namespace Orchard.MessageBus.Brokers.SqlServer {
    /// <summary>
    /// A single connection is maintained, and each subscription will be triggered based on the channel it's listening to
    /// </summary>
    [OrchardFeature("Orchard.MessageBus.SqlServerServiceBroker")]
    public class SqlServerBroker : IMessageBroker, IDisposable {

        private IWorker _worker;
        private bool _initialized;
        private object _synLock = new object();

        private readonly Work<IRepository<MessageRecord>> _messageRecordRepository;
        private readonly Work<IClock> _clock;
        private readonly Func<IWorker> _workerFactory;
        private readonly ShellSettings _shellSettings;
        private readonly Work<IHostNameProvider> _hostNameProvider;

        public SqlServerBroker(
            Work<IRepository<MessageRecord>> messageRecordRepository,
            Work<IClock> clock,
            Work<IHostNameProvider> hostNameProvider,
            Func<IWorker> workerFactory,
            ShellSettings shellSettings
            ) {
            _messageRecordRepository = messageRecordRepository;
            _clock = clock;
            _shellSettings = shellSettings;
            _workerFactory = workerFactory;
            _hostNameProvider = hostNameProvider;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool EnsureInitialized() {
            lock (_synLock) {
                if (!_initialized) {
                    try {
                        // call only once per connectionstring when appdomain starts up
                        Logger.Information("Starting SqlDependency.");
                        SqlDependency.Start(_shellSettings.DataConnectionString);

                        _worker = _workerFactory();
                        _worker.Work();

                        _initialized = true;
                    }
                    catch (Exception e) {
                        Logger.Error(e, "The application doesn't have the permission to request notifications.");
                    }
                }

                return _initialized;
            }
        }

        public void Subscribe(string channel, Action<string, string> handler) {
            if (!EnsureInitialized()) {
                return;
            }

            try {
                lock (_synLock) {
                    _worker.RegisterHandler(channel, handler);
                }
            }
            catch(Exception e) {
                Logger.Error(e, "An error occurred while creating a Worker.");
            }
        }

        public void Publish(string channel, string message) {
            if (!EnsureInitialized()) {
                return;
            }

            // clear old messages on publish to get a single worker
            var oldMessages = _messageRecordRepository.Value
                .Table
                .Where(x => x.CreatedUtc <= _clock.Value.UtcNow.AddHours(-1))
                .ToList();

            foreach (var messageRecord in oldMessages) {
                _messageRecordRepository.Value.Delete(messageRecord);
            }

            _messageRecordRepository.Value.Create(
                new MessageRecord {
                    Channel = channel,
                    Message = message,
                    Publisher = _hostNameProvider.Value.GetHostName(),
                    CreatedUtc = _clock.Value.UtcNow
                }
            );
        }

        public void Dispose() {
            // call only once per connectionstring when appdomain shuts down
            if (!String.IsNullOrWhiteSpace(_shellSettings.DataConnectionString)) {
                SqlDependency.Stop(_shellSettings.DataConnectionString);
            }
        }

        private string GetHostName() {
            // use the current host and the process id as two servers could run on the same machine
            return System.Net.Dns.GetHostName() + ":" + System.Diagnostics.Process.GetCurrentProcess().Id;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.MessageBus.Models;
using Orchard.MessageBus.Services;

namespace Orchard.MessageBus.Brokers.SqlServer {
    public interface IWorker : IDependency {
        void Work();
        void RegisterHandler(string channel, Action<string, string> handler);
    }

    [OrchardFeature("Orchard.MessageBus.SqlServerServiceBroker")]
    public class Worker : IWorker, IRegisteredObject {

        private readonly ShellSettings _shellSettings;
        private readonly IHostNameProvider _hostNameProvider;

        private SqlDependency _dependency;

        private static string commandText = "SELECT Id, Channel, Publisher, Message, CreatedUtc FROM dbo.{0}Orchard_MessageBus_MessageRecord WHERE Id > @Id";
        private static int lastMessageId = 0;
        private bool _stopped;

        private Dictionary<string, List<Action<string, string>>> _handlers = new Dictionary<string, List<Action<string, string>>>();

        public Worker(ShellSettings shellSettings, IHostNameProvider hostNameProvider) {
            _hostNameProvider = hostNameProvider;
            _shellSettings = shellSettings;

            var tablePrefix = _shellSettings.DataTablePrefix;
            if (!String.IsNullOrWhiteSpace(tablePrefix)) {
                tablePrefix += "_";
            }

            commandText = String.Format(commandText, tablePrefix);

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Work() {
            // exit loop if stop notification as been triggered
            if (_stopped) {
                return;
            }

            try {
                IEnumerable<MessageRecord> messages;

                // load and process existing messages
                using (var connection = new SqlConnection(_shellSettings.DataConnectionString)) {
                    connection.Open();

                    var command = CreateCommand(connection);
                    messages = GetMessages(command);
                }

                ProcessMessages(messages);

                // wait for new messages to be available
                WaitForWork();

            }
            catch (Exception e) {
                Logger.Error(e, "An unexpected error occured while monitoring sql dependencies.");
            }
        }

        private void DoWork(object sender, SqlNotificationEventArgs eventArgs) {
            Work();
        }

        private void WaitForWork() {

            using (var connection = new SqlConnection(_shellSettings.DataConnectionString)) {
                connection.Open();

                using (var command = CreateCommand(connection)) {

                    // create a sql depdendency on the table we are monitoring
                    _dependency = new SqlDependency(command);

                    // when new records are present, continue the thread
                    _dependency.OnChange += DoWork;

                    // start monitoring the table
                    command.ExecuteNonQuery();
                }
            }
        }

        private void ProcessMessages(IEnumerable<MessageRecord> messages) {

            if (!messages.Any()) {
                return;
            }

            // if this is the first time it's executed we just need to get the highest Id
            if (lastMessageId == 0) {
                lastMessageId = messages.Max(x => x.Id);
                return;
            }

            // process the messages synchronously and in order of publication
            foreach (var message in messages.OrderBy(x => x.Id)) {

                // save the latest message id so that next time the table is monitored
                // we get notified for new messages
                lastMessageId = message.Id;

                // only process handlers registered for the specific channel
                List<Action<string, string>> channelHandlers;
                if (_handlers.TryGetValue(message.Channel, out channelHandlers)) {

                    var hostName = _hostNameProvider.GetHostName();

                    // execute subscription
                    foreach (var handler in channelHandlers) {

                        // ignore messages sent by the current host
                        if (!message.Publisher.Equals(hostName, StringComparison.OrdinalIgnoreCase)) {
                            handler(message.Channel, message.Message);
                        }

                        // stop processing other messages if stop has been required
                        if (_stopped) {
                            return;
                        }
                    }
                }
            }
        }

        public void Stop(bool immediate) {
            _stopped = true;
        }

        public void RegisterHandler(string channel, Action<string, string> handler) {
            GetHandlersForChannel(channel).Add(handler);
        }

        private List<Action<string, string>> GetHandlersForChannel(string channel) {
            List<Action<string, string>> channelHandlers;

            if (!_handlers.TryGetValue(channel, out channelHandlers)) {
                channelHandlers = new List<Action<string, string>>();
                _handlers.Add(channel, channelHandlers);
            }

            return channelHandlers;
        }

        public SqlCommand CreateCommand(SqlConnection connection) {
            SqlCommand command = new SqlCommand(commandText, connection);

            SqlParameter param = new SqlParameter("@Id", SqlDbType.Int);
            param.Direction = ParameterDirection.Input;
            param.DbType = DbType.Int32;
            param.Value = lastMessageId;
            command.Parameters.Add(param);

            return command;
        }

        public IEnumerable<MessageRecord> GetMessages(SqlCommand command) {
            var result = new List<MessageRecord>();

            try {

                using (var reader = command.ExecuteReader()) {
                    if (reader.HasRows) {
                        while (reader.Read()) {
                            result.Add(new MessageRecord {
                                Id = reader.GetInt32(0),
                                Channel = reader.GetString(1),
                                Publisher = reader.GetString(2),
                                Message = reader.GetString(3),
                                CreatedUtc = reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }
            catch (Exception e) {
                Logger.Error(e, "Could not retreive Sql Broker messages.");
                return Enumerable.Empty<MessageRecord>();
            }

            return result;
        }


    }
}
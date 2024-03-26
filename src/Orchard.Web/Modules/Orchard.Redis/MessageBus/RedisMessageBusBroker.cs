using System;
using System.Collections.Concurrent;
using System.Linq;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.MessageBus.Services;
using Orchard.Redis.Configuration;
using StackExchange.Redis;

namespace Orchard.Redis.MessageBus {

    [OrchardFeature("Orchard.Redis.MessageBus")]
    public class RedisMessageBusBroker : Component, IMessageBroker {

        private readonly IRedisConnectionProvider _redisConnectionProvider;
        
        public const string ConnectionStringKey = "Orchard.Redis.MessageBus";
        private readonly string _connectionString;

        private ConcurrentDictionary<string, ConcurrentBag<Action<string, string>>> _handlers = new ConcurrentDictionary<string, ConcurrentBag<Action<string, string>>>();

        public RedisMessageBusBroker(ShellSettings shellSettings, IRedisConnectionProvider redisConnectionProvider) {
            _redisConnectionProvider = redisConnectionProvider;
            _connectionString = _redisConnectionProvider.GetConnectionString(ConnectionStringKey);
        }

        public IDatabase Database {
            get {
                return _redisConnectionProvider.GetConnection(_connectionString).GetDatabase();
            }
        }

        public void Subscribe(string channel, Action<string, string> handler) {

            try {
                var channelHandlers = _handlers.GetOrAdd(channel, c => {
                    return new ConcurrentBag<Action<string, string>>();
                });

                channelHandlers.Add(handler);

                var sub = _redisConnectionProvider.GetConnection(_connectionString).GetSubscriber();
                sub.Subscribe(channel, (c, m) => {
                    
                    // the message contains the publisher before the first '/'
                    var messageTokens = m.ToString().Split('/');
                    var publisher = messageTokens.FirstOrDefault();
                    var message = messageTokens.Skip(1).FirstOrDefault();

                    if (String.IsNullOrWhiteSpace(publisher)) {
                        return;
                    }

                    // ignore self sent messages
                    if (GetHostName().Equals(publisher, StringComparison.OrdinalIgnoreCase)) {
                        return;
                    }

                    Logger.Debug("Processing {0}", message);
                    handler(c, message);
                });

            }
            catch (Exception e) {
                Logger.Error(e, "An error occurred while subscribing to " + channel);
            }
        }

        public void Publish(string channel, string message) {
            Database.Publish(channel, GetHostName() + "/" + message);
        }

        private string GetHostName() {
            // use the current host and the process id as two servers could run on the same machine
            return System.Net.Dns.GetHostName() + ":" + System.Diagnostics.Process.GetCurrentProcess().Id;
        }

    }
}
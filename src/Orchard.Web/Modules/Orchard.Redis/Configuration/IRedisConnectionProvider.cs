using StackExchange.Redis;

namespace Orchard.Redis.Configuration {

    public interface IRedisConnectionProvider : ISingletonDependency {
        ConnectionMultiplexer GetConnection(string connectionString);
        string GetConnectionString(string service);
    }

}
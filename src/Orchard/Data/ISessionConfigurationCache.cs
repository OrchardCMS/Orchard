using NHibernate.Cfg;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data {
    public interface ISessionConfigurationCache {
        Configuration GetConfiguration(ShellBlueprint shellBlueprint);
    }
}
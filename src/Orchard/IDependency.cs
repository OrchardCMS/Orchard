using Orchard.Localization;
using Orchard.Logging;

namespace Orchard {
    /// <summary>
    /// Base interface for services that are instantiated per unit of work (i.e. web request).
    /// </summary>
    public interface IDependency {
    }

    /// <summary>
    /// Base interface for services that are instantiated per shell/tenant.
    /// </summary>
    public interface ISingletonDependency : IDependency {
    }

    /// <summary>
    /// Base interface for services that are instantiated per usage.
    /// </summary>
    public interface ITransientDependency : IDependency {
    }

    public abstract class Component : IDependency {
        protected Component() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
    }
}

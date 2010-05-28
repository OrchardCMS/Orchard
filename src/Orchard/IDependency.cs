using Orchard.Localization;
using Orchard.Logging;

namespace Orchard {
    public interface IDependency {
    }
    
    public interface ISingletonDependency : IDependency {        
    }

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

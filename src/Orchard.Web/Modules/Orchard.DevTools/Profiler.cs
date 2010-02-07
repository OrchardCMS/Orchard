using Orchard.Environment;

namespace Orchard.DevTools {
    public class Profiler : IOrchardShellEvents {
        public void Activated() {
            HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
        }

        public void Terminating() {
            
        }
    }
}

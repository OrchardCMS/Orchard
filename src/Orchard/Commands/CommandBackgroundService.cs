using Orchard.Tasks;

namespace Orchard.Commands {
    public class CommandBackgroundService : IBackgroundService {
        public void Sweep() {
            // Don't run any background service in command line
        }
    }
}
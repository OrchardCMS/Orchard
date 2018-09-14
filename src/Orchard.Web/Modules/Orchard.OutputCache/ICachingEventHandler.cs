using Orchard.Events;
using System.Text;

namespace Orchard.OutputCache {
    public interface ICachingEventHandler : IEventHandler {
        void KeyGenerated(StringBuilder key);
    }
}
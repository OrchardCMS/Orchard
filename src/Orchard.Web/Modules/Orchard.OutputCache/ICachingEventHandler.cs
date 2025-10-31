using System.Text;
using Orchard.Events;

namespace Orchard.OutputCache
{
    public interface ICachingEventHandler : IEventHandler
    {
        void KeyGenerated(StringBuilder key);
    }
}
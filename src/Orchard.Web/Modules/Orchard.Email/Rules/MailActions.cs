using Orchard.Events;

namespace Orchard.Email.Rules
{

    public interface IActionProvider : IEventHandler
    {
        void Describe(dynamic describe);
    }
}

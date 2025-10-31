using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.Taxonomies.Services
{
    public interface ITermCountProcessor : IEventHandler
    {
        void Process(IEnumerable<int> termPartRecordIds);
    }
}

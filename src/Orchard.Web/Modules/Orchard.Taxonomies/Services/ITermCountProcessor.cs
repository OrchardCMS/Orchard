using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Events;

namespace Orchard.Taxonomies.Services {
    public interface ITermCountProcessor : IEventHandler {
        void Process(IEnumerable<int> termPartRecordIds);
    }
}

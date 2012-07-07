using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.ImportExport.Services {
    public interface ICustomExportStep : IEventHandler {
        void Register(IList<string> steps);
    }
}
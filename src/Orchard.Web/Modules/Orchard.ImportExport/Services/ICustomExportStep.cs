using System;
using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.ImportExport.Services {
    [Obsolete("Implement IRecipeBuilderStep instead.")]
    public interface ICustomExportStep : IEventHandler {
        void Register(IList<string> steps);
    }
}
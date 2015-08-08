using System.Collections.Generic;

namespace Orchard.MultiTenancy.ViewModels
{
    public class CommandsExecuteViewModel {
        public IEnumerable<string> Tenants { get; set; }
    }
}
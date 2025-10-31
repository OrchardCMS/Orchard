using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Orchard.MultiTenancy.ViewModels
{
    public class TenantResetViewModel
    {
        public TenantResetViewModel()
        {
            DatabaseTableNames = Enumerable.Empty<string>();
        }

        [Required]
        public string Name { get; set; }
        public bool DropDatabaseTables { get; set; }
        public IEnumerable<string> DatabaseTableNames { get; set; }
    }
}


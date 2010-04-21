using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.MultiTenancy.ViewModels {
    public class TenantsAddViewModel : BaseViewModel {
        [Required, DisplayName("Tenant Name:")]
        public string Name { get; set; }
        [Required]
        public string DataProvider { get; set; }
        public string ConnectionString { get; set; }
        public string Prefix { get; set; }
    }
}


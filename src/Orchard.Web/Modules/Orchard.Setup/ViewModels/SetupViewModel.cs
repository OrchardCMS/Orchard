using System.ComponentModel.DataAnnotations;
using Orchard.Setup.Annotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.Setup.ViewModels {
    public class SetupViewModel : BaseViewModel {
        [Required, StringLength(70)]
        public string SiteName { get; set; }
        [StringLengthMin(3), StringLength(25)]
        public string AdminUsername { get; set; }
        [Required, StringLengthMin(6), StringLength(20)]
        public string AdminPassword { get; set; }
        [SqlDatabaseConnectionString]
        public string DatabaseConnectionString { get; set; }
        public bool DatabaseOptions { get; set; }
    }
}
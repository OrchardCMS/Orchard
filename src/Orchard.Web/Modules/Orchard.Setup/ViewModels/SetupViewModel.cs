using System.ComponentModel.DataAnnotations;
using Orchard.Setup.Annotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.Setup.ViewModels {
    public class SetupViewModel : BaseViewModel {
        public SetupViewModel() {
            DatabaseOptions = true;
        }

        [Required, StringLength(70)]
        public string SiteName { get; set; }
        [Required, StringLengthMin(3), StringLength(25)]
        public string AdminUsername { get; set; }
        [Required, StringLengthMin(6), StringLength(50)]
        public string AdminPassword { get; set; }
        public bool DatabaseOptions { get; set; }
        [SqlDatabaseConnectionString]
        public string DatabaseConnectionString { get; set; }
    }
}
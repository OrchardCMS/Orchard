using System.ComponentModel.DataAnnotations;
using Orchard.Setup.Annotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.Setup.ViewModels {
    public class SetupViewModel : BaseViewModel {
        public SetupViewModel() {
            DatabaseOptions = true;
        }

        [Required(ErrorMessage = "Site name is required."), StringLength(70, ErrorMessage = "Site name can be no longer than 70 characters.")]
        public string SiteName { get; set; }
        [Required(ErrorMessage = "User name is required."), StringLengthMin(3, ErrorMessage = "User name must be longer than 3 characters."), StringLength(25, ErrorMessage = "User name can be no longer than 25 characters.")]
        public string AdminUsername { get; set; }
        [Required(ErrorMessage = "Password is required."), StringLengthMin(6, ErrorMessage = "Password must be longer than 6 characters."), StringLength(50, ErrorMessage = "Password can be no longer than 50 characters.")]
        public string AdminPassword { get; set; }
        public bool DatabaseOptions { get; set; }
        [SqlDatabaseConnectionString]
        public string DatabaseConnectionString { get; set; }
        public string DatabaseTablePrefix { get; set; }
        public bool DatabaseIsPreconfigured { get; set; }
    }
}
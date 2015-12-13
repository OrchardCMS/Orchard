using System.Collections.Generic;
using Orchard.Recipes.Models;
using Orchard.Setup.Annotations;
using Orchard.Setup.Controllers;

namespace Orchard.Setup.ViewModels {
    public class SetupViewModel  {
        public SetupViewModel() {
        }

        [SiteNameValid(maximumLength: 70)]
        public string SiteName { get; set; }
        [UserNameValid(minimumLength: 3, maximumLength: 25)]
        public string AdminUsername { get; set; }
        [PasswordValid(minimumLength: 7, maximumLength: 50)]
        public string AdminPassword { get; set; }
        [PasswordConfirmationRequired]
        public string ConfirmPassword { get; set; }
        public SetupDatabaseType DatabaseProvider { get; set; }
        
        public string DatabaseConnectionString { get; set; }
        public string DatabaseTablePrefix { get; set; }
        public bool DatabaseIsPreconfigured { get; set; }

        public IEnumerable<Recipe> Recipes { get; set; }
        public string Recipe { get; set; }
        public string RecipeDescription { get; set; }
    }
}
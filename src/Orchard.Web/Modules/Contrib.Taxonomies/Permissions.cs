using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Contrib.Taxonomies {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageTaxonomies = new Permission { Description = "Manage taxonomies", Name = "ManageTaxonomies" };
        public static readonly Permission CreateTaxonomy = new Permission { Description = "Create taxonomy", Name = "CreateTaxonomy", ImpliedBy = new[] { ManageTaxonomies } };
        public static readonly Permission ManageTerms = new Permission { Description = "Manage terms", Name = "ManageTerms", ImpliedBy = new[] { CreateTaxonomy } };
        public static readonly Permission CreateTerm = new Permission { Description = "Create term", Name = "CreateTerm", ImpliedBy = new[] { ManageTerms } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageTaxonomies,
                CreateTaxonomy
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageTaxonomies}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {ManageTaxonomies}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {ManageTaxonomies}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {CreateTaxonomy}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new Permission[0] 
                },
            };
        }

    }
}

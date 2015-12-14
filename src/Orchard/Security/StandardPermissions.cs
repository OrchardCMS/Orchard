using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Security {
    public class StandardPermissions : IPermissionProvider {
        public static readonly Permission AccessAdminPanel = new Permission { Name = "AccessAdminPanel", Description = "Access admin panel" };
        public static readonly Permission AccessFrontEnd = new Permission { Name = "AccessFrontEnd", Description = "Access site front-end" };
        public static readonly Permission SiteOwner = new Permission { Name = "SiteOwner", Description = "Site Owners Permission" }; 

        public Feature Feature {
            get {
                // This is a lie, but it enables the permissions and stereotypes to be created
                return new Feature {
                    Descriptor = new FeatureDescriptor {
                        Id = "Orchard.Framework",
                        Category = "Core",
                        Dependencies = Enumerable.Empty<string>(),
                        Description = "",
                        Extension = new ExtensionDescriptor {
                            Id = "Orchard.Framework"
                        }
                    },
                    ExportedTypes = Enumerable.Empty<Type>()
                };
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                AccessAdminPanel,
                AccessFrontEnd,
                SiteOwner
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {SiteOwner, AccessAdminPanel}
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                    Permissions = new[] {AccessFrontEnd}
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] {AccessFrontEnd}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {AccessAdminPanel}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {AccessAdminPanel}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {AccessAdminPanel}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {AccessAdminPanel}
                },
            };
        }

    }
}
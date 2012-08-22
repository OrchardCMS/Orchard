using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.CustomForms.Models;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.CustomForms {
    public class Permissions : IPermissionProvider {
        private static readonly Permission SubmitAnyForm = new Permission { Description = "Submit any forms", Name = "Submit" };
        private static readonly Permission SubmitForm = new Permission { Description = "Submit {0} forms", Name = "Submit_{0}", ImpliedBy = new[] { SubmitAnyForm } };

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        
        public virtual Feature Feature { get; set; }

        public Permissions(IContentDefinitionManager contentDefinitionManager, IContentManager contentManager) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
        }

        public IEnumerable<Permission> GetPermissions() {
            // manage rights only for Creatable types
            var formTypes = _contentManager.Query<CustomFormPart>().List().Select(x => x.ContentType).Distinct();
            
            foreach (var contentType in formTypes) {
                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
                if(typeDefinition == null) {
                    continue;
                }

                yield return CreateSubmitPermission(typeDefinition);
            }

            yield return SubmitAnyForm;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { SubmitAnyForm }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { SubmitAnyForm }
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] { SubmitAnyForm }
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] { SubmitAnyForm }
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] { SubmitAnyForm }
                }
            };
        }

        /// <summary>
        /// Generates a permission dynamically for a content type
        /// </summary>
        public static Permission CreateSubmitPermission(ContentTypeDefinition typeDefinition) {
            return new Permission {
                Name = String.Format(SubmitForm.Name, typeDefinition.Name),
                Description = String.Format(SubmitForm.Description, typeDefinition.DisplayName),
                Category = "Custom Forms",
                ImpliedBy = new [] { SubmitForm }
            };
        }

        /// <summary>
        /// Generates a permission dynamically for a content type
        /// </summary>
        public static Permission CreateSubmitPermission(string contentType) {
            return new Permission {
                Name = String.Format(SubmitForm.Name, contentType),
                Description = String.Format(SubmitForm.Description, contentType),
                Category = "Custom Forms",
                ImpliedBy = new[] { SubmitForm }
            };
        }

    }
}

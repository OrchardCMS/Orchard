using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Core.Contents {
    public class DynamicPermissions : IPermissionProvider {
        public static readonly Permission ManageContentType = new Permission { Name = "ManageContentType{0}", Description = "Manage {0}" };

        private readonly IContentDefinitionManager _contentDefinitionManager;

        public virtual Feature Feature { get; set; }

        public DynamicPermissions(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public IEnumerable<Permission> GetPermissions() {
            // manage rights only for Creatable types
            var creatableTypes = _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable);

            foreach(var typeDefinition in creatableTypes) {
                yield return CreateDynamicPersion(ManageContentType, typeDefinition);
            }
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = _contentDefinitionManager.ListTypeDefinitions().Select(typeDefinition => CreateDynamicPersion(ManageContentType, typeDefinition))
                }
            };
        }

        private static Permission CreateDynamicPersion(Permission template, ContentTypeDefinition typeDefinition) {
            return new Permission {
                Name = String.Format(template.Name, typeDefinition.Name),
                Description = String.Format(template.Description, typeDefinition.DisplayName),
                Category = typeDefinition.DisplayName
            };
        }
    }
}

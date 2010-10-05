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
        public static readonly Permission PublishOthersContent = new Permission { Description = "Publish or unpublish {0} for others", Name = "PublishOthers_{0}", ImpliedBy = new[] { Permissions.PublishOthersContent } };
        public static readonly Permission PublishContent = new Permission { Description = "Publish or unpublish {0}", Name = "Publish_{0}", ImpliedBy = new[] { PublishOthersContent, Permissions.PublishContent } };
        public static readonly Permission EditOthersContent = new Permission { Description = "Edit {0} for others", Name = "EditOthers_{0}", ImpliedBy = new[] { PublishOthersContent, Permissions.EditOthersContent } };
        public static readonly Permission EditContent = new Permission { Description = "Edit {0}", Name = "EditContent", ImpliedBy = new[] { EditOthersContent, PublishContent, Permissions.EditContent } };
        public static readonly Permission DeleteOthersContent = new Permission { Description = "Delete {0} for others", Name = "DeleteOthers_{0}", ImpliedBy = new[] { Permissions.DeleteOthersContent } };
        public static readonly Permission DeleteContent = new Permission { Description = "Delete {0}", Name = "Delete_{0}", ImpliedBy = new[] { DeleteOthersContent, Permissions.DeleteContent } };

        public static readonly Permission[] PermissionTemplates = new[] {PublishOthersContent, PublishContent, EditOthersContent, EditContent, DeleteOthersContent, DeleteContent};

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
                foreach ( var permissionTemplate in PermissionTemplates ) {
                    yield return CreateDynamicPersion(permissionTemplate, typeDefinition);
                }
            }
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

        public static Permission CreateDynamicPersion(Permission template, ContentTypeDefinition typeDefinition) {
            return new Permission {
                Name = String.Format(template.Name, typeDefinition.Name),
                Description = String.Format(template.Description, typeDefinition.DisplayName),
                Category = typeDefinition.DisplayName,
                ImpliedBy = (template.ImpliedBy ?? new Permission[0]).Select(t => CreateDynamicPersion(t, typeDefinition))
            };
        }
    }
}

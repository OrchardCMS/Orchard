using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.DynamicForms.Elements;
using Orchard.Environment.Extensions.Models;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;
using Orchard.Localization;
using Orchard.Security.Permissions;
using Orchard.Layouts.Helpers;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentPermissions.Settings;

namespace Orchard.DynamicForms {
    public class DynamicPermissions : IPermissionProvider {
        //Dynamic Permissions per content type
        public static readonly Permission SubmitAnyFormInAContentType = new Permission { Description = "Submit any form in {0}", Name = "Submit_contentType_{0}", ImpliedBy = new[] { Permissions.SubmitAnyForm } };
        public static readonly Permission SubmitAnyFormInAContentTypeForModifyData = new Permission { Description = "Submit any form in {0} for modify data own by others", Name = "SubmitForModify_contentType_{0}", ImpliedBy = new[] { Permissions.SubmitAnyFormForModifyData } };
        public static readonly Permission SubmitAnyFormInAContentTypeForModifyOwnData = new Permission { Description = "Submit any form in {0} for modify own data", Name = "SubmitForModifyOwn_contentType_{0}", ImpliedBy = new[] { Permissions.SubmitAnyFormForModifyData, Permissions.SubmitAnyFormForModifyOwnData, SubmitAnyFormInAContentTypeForModifyData } };
        public static readonly Permission SubmitAnyFormInAContentTypeForDeleteData = new Permission { Description = "Submit any form in {0} for delete data own by others", Name = "SubmitForDelete_contentType_{0}", ImpliedBy = new[] { Permissions.SubmitAnyFormForDeleteData } };
        public static readonly Permission SubmitAnyFormInAContentTypeForDeleteOwnData = new Permission { Description = "Submit any form in {0} for delete own data", Name = "SubmitForDeleteOwn_contentType_{0}", ImpliedBy = new[] { Permissions.SubmitAnyFormForDeleteData, Permissions.SubmitAnyFormForDeleteOwnData, SubmitAnyFormInAContentTypeForDeleteData } };

        //Dynamic Permissions per content item
        public static readonly Permission SubmitAnyFormInAContentItem = new Permission { Description = "Submit any form", Name = "Submit_contentType_{0}_contentItem_{1}", ImpliedBy = new[] { Permissions.SubmitAnyForm } };
        public static readonly Permission SubmitAnyFormInAContentItemForModifyData = new Permission { Description = "Submit any form for modify data own by others", Name = "SubmitForModify_contentType_{0}_contentItem_{1}", ImpliedBy = new[] { Permissions.SubmitAnyFormForModifyData } };
        public static readonly Permission SubmitAnyFormInAContentItemForModifyOwnData = new Permission { Description = "Submit any form for modify own data", Name = "SubmitForModifyOwn_contentType_{0}_contentItem_{1}", ImpliedBy = new[] { Permissions.SubmitAnyFormForModifyData, Permissions.SubmitAnyFormForModifyOwnData, SubmitAnyFormInAContentItemForModifyData } };
        public static readonly Permission SubmitAnyFormInAContentItemForDeleteData = new Permission { Description = "Submit any form for delete data own by others", Name = "SubmitForDelete_contentType_{0}_contentItem_{1}", ImpliedBy = new[] { Permissions.SubmitAnyFormForDeleteData } };
        public static readonly Permission SubmitAnyFormInAContentItemForDeleteOwnData = new Permission { Description = "Submit any form for delete own data", Name = "SubmitForDeleteOwn_contentType_{0}_contentItem_{1}", ImpliedBy = new[] { Permissions.SubmitAnyFormForDeleteData, Permissions.SubmitAnyFormForDeleteOwnData, SubmitAnyFormInAContentItemForDeleteData } };

        private static readonly Permission SubmitAFormInAContentItem = new Permission { Description = "Submit {2} form", Name = "Submit_contentType_{0}_contentItem_{1}_form_{2}", ImpliedBy = new[] { Permissions.SubmitAnyForm, SubmitAnyFormInAContentItem } };
        private static readonly Permission SubmitAFormInAContentItemForModifyData = new Permission { Description = "Submit {2} form for modify data own by others", Name = "SubmitForModify_contentType_{0}_contentItem_{1}_form_{2}", ImpliedBy = new[] { Permissions.SubmitAnyFormForModifyData, SubmitAnyFormInAContentItemForModifyData } };
        private static readonly Permission SubmitAFormInAContentItemForModifyOwnData = new Permission { Description = "Submit {2} form for modify own data", Name = "SubmitForModifyOwn_contentType_{0}_contentItem_{1}_form_{2}", ImpliedBy = new[] { Permissions.SubmitAnyFormForModifyData, Permissions.SubmitAnyFormForModifyOwnData, SubmitAnyFormInAContentItemForModifyData, SubmitAnyFormInAContentItemForModifyOwnData, SubmitAFormInAContentItemForModifyData } };
        private static readonly Permission SubmitAFormInAContentItemForDeleteData = new Permission { Description = "Submit {2} form for delete data own by others", Name = "SubmitForDelete_contentType_{0}_contentItem_{1}_form_{2}", ImpliedBy = new[] { Permissions.SubmitAnyFormForDeleteData, SubmitAnyFormInAContentItemForDeleteData } };
        private static readonly Permission SubmitAFormInAContentItemForDeleteOwnData = new Permission { Description = "Submit {2} form for delete own data", Name = "SubmitForDeleteOwn_contentType_{0}_contentItem_{1}_form_{2}", ImpliedBy = new[] { Permissions.SubmitAnyFormForDeleteData, Permissions.SubmitAnyFormForDeleteOwnData, SubmitAnyFormInAContentItemForDeleteData, SubmitAnyFormInAContentItemForDeleteOwnData, SubmitAFormInAContentItemForDeleteData } };

        public static readonly Dictionary<string, Permission> PermissionTemplates = new Dictionary<string, Permission> {
            {Permissions.SubmitAnyForm.Name, SubmitAnyFormInAContentType},
            {Permissions.SubmitAnyFormForModifyData.Name, SubmitAnyFormInAContentTypeForModifyData},
            {Permissions.SubmitAnyFormForModifyOwnData.Name, SubmitAnyFormInAContentTypeForModifyOwnData},
            {Permissions.SubmitAnyFormForDeleteData.Name, SubmitAnyFormInAContentTypeForDeleteData},
            {Permissions.SubmitAnyFormForDeleteOwnData.Name, SubmitAnyFormInAContentTypeForDeleteOwnData},
            {SubmitAnyFormInAContentType.Name, SubmitAnyFormInAContentItem},
            {SubmitAnyFormInAContentTypeForModifyData.Name, SubmitAnyFormInAContentItemForModifyData},
            {SubmitAnyFormInAContentTypeForModifyOwnData.Name, SubmitAnyFormInAContentItemForModifyOwnData},
            {SubmitAnyFormInAContentTypeForDeleteData.Name, SubmitAnyFormInAContentItemForDeleteData},
            {SubmitAnyFormInAContentTypeForDeleteOwnData.Name, SubmitAnyFormInAContentItemForDeleteOwnData},
            {SubmitAnyFormInAContentItem.Name, SubmitAFormInAContentItem},
            {SubmitAnyFormInAContentItemForModifyData.Name, SubmitAFormInAContentItemForModifyData},
            {SubmitAnyFormInAContentItemForModifyOwnData.Name, SubmitAFormInAContentItemForModifyOwnData},
            {SubmitAnyFormInAContentItemForDeleteData.Name, SubmitAFormInAContentItemForDeleteData},
            {SubmitAnyFormInAContentItemForDeleteOwnData.Name, SubmitAFormInAContentItemForDeleteOwnData}
        };

        public static readonly List<Permission> PermissionPerContentTypeTemplates = new List<Permission> { SubmitAnyFormInAContentType,
            SubmitAnyFormInAContentTypeForModifyData, SubmitAnyFormInAContentTypeForModifyOwnData,
            SubmitAnyFormInAContentTypeForDeleteData, SubmitAnyFormInAContentTypeForDeleteOwnData
        };

        public static readonly List<Permission> PermissionPerContentItemTemplates = new List<Permission> { SubmitAnyFormInAContentItem,
            SubmitAnyFormInAContentItemForModifyData, SubmitAnyFormInAContentItemForModifyOwnData,
            SubmitAnyFormInAContentItemForDeleteData, SubmitAnyFormInAContentItemForDeleteOwnData
        };
    

        public static readonly List<Permission> PermissionPerContentItemWithFormsTemplates = new List<Permission> {
            SubmitAFormInAContentItem, SubmitAFormInAContentItemForModifyData, SubmitAFormInAContentItemForModifyOwnData,
            SubmitAFormInAContentItemForDeleteData, SubmitAFormInAContentItemForDeleteOwnData
        };

        public virtual Feature Feature { get; set; }

        public Localizer T { get; set; }

        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILayoutSerializer _serializer;

        public DynamicPermissions(IContentManager contentManager, ILayoutSerializer serializer, IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _serializer = serializer;
            T = NullLocalizer.Instance;
        }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

        public IEnumerable<Permission> GetPermissions() {

            var securableTypes = _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Securable && ctd.Parts.Any(p => p.PartDefinition.Name == "LayoutPart"));

            //Content type level permissions
            foreach (var typeDefinition in securableTypes) {
                foreach (var permissionTemplate in PermissionPerContentTypeTemplates) {
                    yield return CreateDynamicPermission(permissionTemplate, typeDefinition, null, null, T, _contentManager);
                }
            }

            var securableTypesbyContentItems = _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Settings.GetModel<ContentPermissionsTypeSettings>().SecurableContentItems && ctd.Parts.Any(p => p.PartDefinition.Name == "LayoutPart"))
                .ToList();

            foreach (var layoutPart in _contentManager.Query<LayoutPart, LayoutPartRecord>(securableTypesbyContentItems.Select(t => t.Name).ToArray()).List()) {
                //Content item level permissions
                foreach (var permissionTemplate in PermissionPerContentItemTemplates) {
                    yield return CreateDynamicPermission(permissionTemplate, layoutPart.ContentItem.TypeDefinition, layoutPart.ContentItem, null, T, _contentManager);
                }
                //Form level permissions
                foreach (var form in GetAllForms(layoutPart)) {
                    foreach (var permissionTemplate in PermissionPerContentItemWithFormsTemplates) {
                        yield return CreateDynamicPermission(permissionTemplate, layoutPart.ContentItem.TypeDefinition, layoutPart.ContentItem, form.Name, T, _contentManager);
                    }
                }
            }

        }

        IEnumerable<Form> GetAllForms(LayoutPart layoutPart) {
            var elements = _serializer.Deserialize(layoutPart.LayoutData, new DescribeElementsContext { Content = layoutPart });
            return elements.Flatten().Where(x => x is Form).Cast<Form>();
        }

        /// <summary>
        /// Generates a permission dynamically for a content item
        /// </summary>
        public static Permission CreateDynamicPermission(Permission template, ContentTypeDefinition typeDefinition, IContent content, string formName, Localizer T, IContentManager contentManager) {
            var identity = (content != null) ? contentManager.GetItemMetadata(content).Identity.ToString() : "";
            var displayText = (content != null) ? contentManager.GetItemMetadata(content).DisplayText : "";

            return new Permission {
                Name = String.Format(template.Name, typeDefinition.DisplayName, identity, formName),
                Description = String.Format(template.Description, typeDefinition.DisplayName, displayText, formName),
                Category = GetDynamicCategory(template, displayText, typeDefinition.DisplayName, T),
                ImpliedBy = CreateDynamicImpliedBy(template.ImpliedBy, template, typeDefinition, content, formName, T, contentManager)
            };
        }

        private static string GetDynamicCategory(Permission template, string displayText, string typeDefinition, Localizer T) {
            if (template.Name == SubmitAnyFormInAContentItem.Name
                || template.Name == SubmitAnyFormInAContentItemForModifyData.Name
                || template.Name == SubmitAnyFormInAContentItemForModifyOwnData.Name
                || template.Name == SubmitAnyFormInAContentItemForDeleteData.Name
                || template.Name == SubmitAnyFormInAContentItemForDeleteOwnData.Name
                || template.Name == SubmitAFormInAContentItem.Name
                || template.Name == SubmitAFormInAContentItemForModifyData.Name
                || template.Name == SubmitAFormInAContentItemForModifyOwnData.Name
                || template.Name == SubmitAFormInAContentItemForDeleteData.Name
                || template.Name == SubmitAFormInAContentItemForDeleteOwnData.Name
                )
                return T("{0} - {1}", typeDefinition, displayText).ToString();
            else if (template.Name == SubmitAnyFormInAContentType.Name
                || template.Name == SubmitAnyFormInAContentTypeForModifyData.Name
                || template.Name == SubmitAnyFormInAContentTypeForModifyOwnData.Name
                || template.Name == SubmitAnyFormInAContentTypeForDeleteData.Name
                || template.Name == SubmitAnyFormInAContentTypeForDeleteOwnData.Name
                )
                return T("{0}", typeDefinition, displayText).ToString();
            else
                return null;
        }

        private static IEnumerable<Permission> CreateDynamicImpliedBy(IEnumerable<Permission> impliedBy, Permission template, ContentTypeDefinition typeDefinition, IContent content, string formName, Localizer T, IContentManager contentManager) {
            var result = new List<Permission>();
            if (impliedBy == null)
                return result;
            foreach (var permission in impliedBy) {

                if (permission.Name == Permissions.ManageForms.Name || permission.Name == Permissions.SubmitAnyForm.Name 
                    || permission.Name == Permissions.SubmitAnyFormForModifyData.Name || permission.Name == Permissions.SubmitAnyFormForModifyOwnData.Name
                    || permission.Name == Permissions.SubmitAnyFormForDeleteData.Name || permission.Name == Permissions.SubmitAnyFormForDeleteOwnData.Name)
                    result.Add(permission);
                else
                    result.Add(CreateDynamicPermission(permission, typeDefinition, content, formName, T, contentManager));
            }
            return result;
        }

        
        /// <summary>
        /// Returns a dynamic permission for a content item, based on a static form content permissions
        /// </summary>
        public static Permission ConvertToDynamicPermission(Permission permission, ContentTypeDefinition typeDefinition, IContent content, string formName, IContentManager contentManager) {
            if (PermissionTemplates.ContainsKey(permission.Name))
                return PermissionTemplates[permission.Name];
            else {
                var identity = (content != null) ? contentManager.GetItemMetadata(content).Identity.ToString() : "";
                var displayText = (content != null) ? contentManager.GetItemMetadata(content).DisplayText : "";
                foreach (string dynamicPermissionTemplateWithForms in PermissionPerContentTypeTemplates.Union(PermissionPerContentItemTemplates).Select(p=>p.Name)) {
                    if (permission.Name == String.Format(dynamicPermissionTemplateWithForms, typeDefinition.DisplayName, identity, formName))
                        return PermissionTemplates[dynamicPermissionTemplateWithForms];
                }
            }
            return null;
        }
    }
}

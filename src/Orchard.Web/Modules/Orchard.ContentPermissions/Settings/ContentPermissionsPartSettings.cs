using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.ContentPermissions.ViewModels;

namespace Orchard.ContentPermissions.Settings {
    public class ContentPermissionsPartSettings {
        public string View { get; set; }
        public string ViewOwn { get; set; }
        public string Publish { get; set; }
        public string PublishOwn { get; set; }
        public string Edit { get; set; }
        public string EditOwn { get; set; }
        public string Delete { get; set; }
        public string DeleteOwn { get; set; }

        public string DisplayedRoles { get; set; }
    }

    public class ViewPermissionsSettingsHooks : ContentDefinitionEditorEventsBase {
        private readonly IAuthorizer _authorizer;
        private readonly IAuthorizationService _authorizationService;
        private readonly IRoleService _roleService;

        public ViewPermissionsSettingsHooks(
            IAuthorizer authorizer, 
            IAuthorizationService authorizationService, 
            IRoleService roleService
            ) {
            _authorizer = authorizer;
            _authorizationService = authorizationService;
            _roleService = roleService;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ContentPermissionsPart")
                yield break;

            // ensure the current user is allowed to define permissions
            if(!_authorizer.Authorize(Permissions.GrantPermission)) {
                yield break;
            }

            var settings = definition.Settings.TryGetModel<ContentPermissionsPartSettings>();

            var allRoles = _roleService.GetRoles().Select(x => x.Name).OrderBy(x => x).ToList();

            // copy defaults if new type
            if(settings == null) {
                settings = new ContentPermissionsPartSettings {
                    View = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(Core.Contents.Permissions.ViewContent, UserSimulation.Create(x), null) })),
                    ViewOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(Core.Contents.Permissions.ViewOwnContent, UserSimulation.Create(x), null) })),
                    Publish = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(Core.Contents.Permissions.PublishContent, UserSimulation.Create(x), null) })),
                    PublishOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(Core.Contents.Permissions.PublishOwnContent, UserSimulation.Create(x), null) })),
                    Edit = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(Core.Contents.Permissions.EditContent, UserSimulation.Create(x), null) })),
                    EditOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(Core.Contents.Permissions.EditOwnContent, UserSimulation.Create(x), null) })),
                    Delete = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(Core.Contents.Permissions.DeleteContent, UserSimulation.Create(x), null) })),
                    DeleteOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(Core.Contents.Permissions.DeleteOwnContent, UserSimulation.Create(x), null) })),
                    DisplayedRoles = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = true })),
                };
            }

            var model = new ContentPermissionsPartViewModel {
                ViewRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.View),
                ViewOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.ViewOwn),
                PublishRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.Publish),
                PublishOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.PublishOwn),
                EditRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.Edit),
                EditOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.EditOwn),
                DeleteRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.Delete),
                DeleteOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.DeleteOwn),
                AllRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.DisplayedRoles)
            };

            // disable permissions the current user doesn't have
            model.ViewRoles = model.ViewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.ViewContent) }).ToList();
            model.ViewOwnRoles = model.ViewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.ViewOwnContent) }).ToList();
            model.PublishRoles = model.PublishRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.PublishContent) }).ToList();
            model.PublishOwnRoles = model.PublishOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.PublishOwnContent) }).ToList();
            model.EditRoles = model.EditRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.EditContent) }).ToList();
            model.EditOwnRoles = model.EditOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.EditOwnContent) }).ToList();
            model.DeleteRoles = model.DeleteRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.DeleteContent) }).ToList();
            model.DeleteOwnRoles = model.DeleteOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.DeleteOwnContent) }).ToList();

            // initialize default value
            model.ViewRoles = model.ViewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.ViewContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.ViewOwnRoles = model.ViewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.ViewOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PublishRoles = model.PublishRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.PublishContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PublishOwnRoles = model.PublishOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.PublishOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.EditRoles = model.EditRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.EditContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.EditOwnRoles = model.EditOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.EditOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.DeleteRoles = model.DeleteRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.DeleteContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.DeleteOwnRoles = model.DeleteOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.DeleteOwnContent, UserSimulation.Create(x.Role), null) }).ToList();

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ContentPermissionsPart")
                yield break;

            if (!_authorizer.Authorize(Permissions.GrantPermission)) {
                yield break;
            }

            var allRoles = _roleService.GetRoles().Select(x => x.Name).OrderBy(x => x).ToList();

            var model = new ContentPermissionsPartViewModel();

            updateModel.TryUpdateModel(model, "ContentPermissionsPartViewModel", null, null);
            
            // update permissions only for those the current user is granted
            if ( _authorizer.Authorize(Core.Contents.Permissions.ViewContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.View", ContentPermissionsPartViewModel.SerializePermissions(model.ViewRoles));
            }

            if (_authorizer.Authorize(Core.Contents.Permissions.ViewOwnContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.ViewOwn", ContentPermissionsPartViewModel.SerializePermissions(model.ViewOwnRoles));
            }

            if (_authorizer.Authorize(Core.Contents.Permissions.PublishContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.Publish", ContentPermissionsPartViewModel.SerializePermissions(model.PublishRoles));
            }

            if (_authorizer.Authorize(Core.Contents.Permissions.PublishOwnContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.PublishOwn", ContentPermissionsPartViewModel.SerializePermissions(model.PublishOwnRoles));
            }

            if (_authorizer.Authorize(Core.Contents.Permissions.EditContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.Edit", ContentPermissionsPartViewModel.SerializePermissions(model.EditRoles));
            }

            if (_authorizer.Authorize(Core.Contents.Permissions.EditOwnContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.EditOwn", ContentPermissionsPartViewModel.SerializePermissions(model.EditOwnRoles));
            }

            if (_authorizer.Authorize(Core.Contents.Permissions.DeleteContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.Delete", ContentPermissionsPartViewModel.SerializePermissions(model.DeleteRoles));
            }

            if (_authorizer.Authorize(Core.Contents.Permissions.DeleteOwnContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.DeleteOwn", ContentPermissionsPartViewModel.SerializePermissions(model.DeleteOwnRoles));
            }

            builder.WithSetting("ContentPermissionsPartSettings.DisplayedRoles", ContentPermissionsPartViewModel.SerializePermissions(model.AllRoles));

            // disable permissions the current user doesn't have
            model.ViewRoles = model.ViewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.ViewContent) }).ToList();
            model.ViewOwnRoles = model.ViewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.ViewOwnContent) }).ToList();
            model.PublishRoles = model.PublishRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.PublishContent) }).ToList();
            model.PublishOwnRoles = model.PublishOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.PublishOwnContent) }).ToList();
            model.EditRoles = model.EditRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.EditContent) }).ToList();
            model.EditOwnRoles = model.EditOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.EditOwnContent) }).ToList();
            model.DeleteRoles = model.DeleteRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.DeleteContent) }).ToList();
            model.DeleteOwnRoles = model.DeleteOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(Core.Contents.Permissions.DeleteOwnContent) }).ToList();

            // initialize default value
            model.ViewRoles = model.ViewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.ViewContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.ViewOwnRoles = model.ViewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.ViewOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PublishRoles = model.PublishRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.PublishContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PublishOwnRoles = model.PublishOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.PublishOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.EditRoles = model.EditRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.EditContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.EditOwnRoles = model.EditOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.EditOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.DeleteRoles = model.DeleteRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.DeleteContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.DeleteOwnRoles = model.DeleteOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(Core.Contents.Permissions.DeleteOwnContent, UserSimulation.Create(x.Role), null) }).ToList();

            yield return DefinitionTemplate(model);
        }
    }
}
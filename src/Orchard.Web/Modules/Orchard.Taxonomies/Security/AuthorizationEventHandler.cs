using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            Permission permission = context.Permission;
            // adjusting permissions only if the content is not securable
            if (!context.Granted &&
                context.Content.Is<ICommonPart>()) {
                var typeDefinition = context.Content.ContentItem.TypeDefinition;
                if (!typeDefinition.Settings.GetModel<ContentTypeSettings>().Securable) {
                    if (context.Content.Is<TermPart>()) {
                        if (context.Permission == Core.Contents.Permissions.CreateContent) {
                            permission = Permissions.CreateTerm;
                        }
                        else if (context.Permission == TryGetOwnerVariation(Core.Contents.Permissions.EditContent, context)) {
                            permission = Permissions.EditTerm;
                        }
                        else if (context.Permission == TryGetOwnerVariation(Core.Contents.Permissions.PublishContent, context)) {
                            permission = Permissions.EditTerm;
                        }
                        else if (context.Permission == TryGetOwnerVariation(Core.Contents.Permissions.DeleteContent, context)) {
                            permission = Permissions.DeleteTerm;
                        }
                    }
                    else if (context.Content.Is<TaxonomyPart>()) {
                        if (context.Permission == Core.Contents.Permissions.CreateContent) {
                            permission = Permissions.CreateTaxonomy;
                        }
                        else if (context.Permission == TryGetOwnerVariation(Core.Contents.Permissions.EditContent, context)) {
                            permission = Permissions.CreateTaxonomy;
                        }
                        else if (context.Permission == TryGetOwnerVariation(Core.Contents.Permissions.PublishContent, context)) {
                            permission = Permissions.CreateTaxonomy;
                        }
                        else if (context.Permission == TryGetOwnerVariation(Core.Contents.Permissions.DeleteContent, context)) {
                            permission = Permissions.ManageTaxonomies;
                        }
                    }
                    if (permission != context.Permission) {
                        context.Permission = permission;
                        context.Adjusted = true;
                    }
                }
            }
        }

        private static bool HasOwnership(IUser user, IContent content) {
            if (user == null || content == null)
                return false;

            var common = content.As<ICommonPart>();
            if (common == null || common.Owner == null)
                return false;

            return user.Id == common.Owner.Id;
        }

        private static Permission TryGetOwnerVariation(Permission permission, CheckAccessContext context) {
            if (HasOwnership(context.User, context.Content)) {
                if (permission.Name == Core.Contents.Permissions.PublishContent.Name)
                    return Core.Contents.Permissions.PublishOwnContent;
                if (permission.Name == Core.Contents.Permissions.EditContent.Name)
                    return Core.Contents.Permissions.EditOwnContent;
                if (permission.Name == Core.Contents.Permissions.DeleteContent.Name)
                    return Core.Contents.Permissions.DeleteOwnContent;
                if (permission.Name == Core.Contents.Permissions.ViewContent.Name)
                    return Core.Contents.Permissions.ViewOwnContent;
                if (permission.Name == Core.Contents.Permissions.PreviewContent.Name)
                    return Core.Contents.Permissions.PreviewOwnContent;

                return null;
            }
            else {
                return permission;
            }
        }
    }
}

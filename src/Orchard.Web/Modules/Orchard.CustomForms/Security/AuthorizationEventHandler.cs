using Orchard.ContentManagement;
using Orchard.CustomForms.Models;
using Orchard.Security;

namespace Orchard.CustomForms.Security {
    /// <summary>
    /// Alters the Edit permission requested by the Contents module before editing a form. Returns a Submit permission instead.
    /// </summary>
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if (!context.Granted 
                && context.Permission.Name == Orchard.Core.Contents.Permissions.EditContent.Name 
                && context.Content != null
                && context.Content.ContentItem.ContentType == "CustomForm") {

                context.Adjusted = true;
                context.Permission = Permissions.CreateSubmitPermission(context.Content.ContentItem.As<CustomFormPart>().ContentType);
            }
        }
    }
}
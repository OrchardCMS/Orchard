using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Users.Models;

namespace Orchard.Users.Controllers {
    [UsedImplicitly]
    public class UserDriver : ContentItemDriver<User> {
        public readonly static ContentType ContentType = new ContentType {
            Name = "user",
            DisplayName = "User Profile"
        };

        protected override bool UseDefaultTemplate { get { return true; } }

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string GetDisplayText(User item) {
            //TEMP: need a "display name" probably... showing login info likely not a best practice...
            return item.UserName;
        }

        protected override RouteValueDictionary GetEditorRouteValues(User item) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Users"},
                                                {"Controller", "Admin"},
                                                {"Action", "Edit"},
                                                {"Id", item.ContentItem.Id},
                                            };
        }
    }
}

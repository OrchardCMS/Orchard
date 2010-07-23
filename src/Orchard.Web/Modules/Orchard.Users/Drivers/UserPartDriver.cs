using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Users.Models;

namespace Orchard.Users.Drivers {
    [UsedImplicitly]
    public class UserPartDriver : ContentItemDriver<UserPart> {
        public readonly static ContentType ContentType = new ContentType {
                                                                             Name = "User",
                                                                             DisplayName = "User Profile"
                                                                         };

        protected override bool UseDefaultTemplate { get { return true; } }

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string GetDisplayText(UserPart item) {
            //TEMP: need a "display name" probably... showing login info likely not a best practice...
            return item.UserName;
        }

        public override RouteValueDictionary GetEditorRouteValues(UserPart item) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Users"},
                                                {"Controller", "Admin"},
                                                {"Action", "Edit"},
                                                {"Id", item.ContentItem.Id},
                                            };
        }
    }
}